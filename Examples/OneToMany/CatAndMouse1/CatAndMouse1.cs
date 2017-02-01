// License: The MIT License (MIT) Copyright (c) 2010..2012 Barend Gehrels

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Data.SqlClient;

using Stormy;
using CatAndMouse;

/* 

create table cat(CatId uniqueidentifier primary key, name nvarchar(16), sex nvarchar(1), weight real);
insert into cat(CatId, name, sex, weight) values(NEWID(), 'Sockington', 'm', 5.5);
insert into cat(CatId, name, sex, weight) values(NEWID(), 'Scarlett', 'f', 5.0);
insert into cat(CatId, name, sex, weight) values(NEWID(), 'Tom', 'm', 5.2);

create table mouse(MouseId uniqueidentifier primary key, name nvarchar(16), color int);
insert into mouse(MouseId, name, color) values(NEWID(), 'Jerry', 3);
insert into mouse(MouseId, name, color) values(NEWID(), 'Speedy', 3);
insert into mouse(MouseId, name, color) values(NEWID(), 'Mickey', 4);

-- Relation:
create table chasing(CatId uniqueidentifier not null, MouseId uniqueidentifier not null);
insert into chasing values((select CatId from cat where name='Tom'),(select MouseId from mouse where name='Jerry'));
insert into chasing values((select CatId from cat where name='Sockington'),(select MouseId from mouse where name='Jerry'));
insert into chasing values((select CatId from cat where name='Sockington'),(select MouseId from mouse where name='Speedy'));
insert into chasing values((select CatId from cat where name='Scarlett'),(select MouseId from mouse where name='Speedy'));
insert into chasing values((select CatId from cat where name='Scarlett'),(select MouseId from mouse where name='Mickey'));

 */



namespace CatAndMouse1
{

    // ORM Conversions:
    public class CatConverter : ISelectable<Cat>
    {
        public Cat ApplySelect(IDataReader reader)
        {
            Cat cat = new Cat();
            cat.Id = (Guid)reader["CatId"];
            cat.Name = (string)reader["name"];
            cat.Sex = (((string)reader["sex"]) + " ")[0];
            cat.Weight = (float)reader["weight"];
            return cat;
        }
    }


    public class MouseConverter : ISelectable<Mouse>
    {
        public Mouse ApplySelect(IDataReader reader)
        {
            Mouse mouse = new Mouse();
            mouse.Id = (Guid)reader["MouseId"];
            mouse.Name = (string)reader["name"];
            mouse.Color = (AnimalColor)reader["color"];
            return mouse;
        }
    }

    // Method 1: read link table and add links
    public class ChaseConverter : ISelectable<object>
    {
        IEnumerable<Cat> m_cats;
        IEnumerable<Mouse> m_mice;

        public ChaseConverter(IEnumerable<Cat> cats,IEnumerable<Mouse> mice)
        {
            m_cats = cats;
            m_mice = mice;
        }
        public object ApplySelect(IDataReader reader)
        {
            Guid mouseId = (Guid)reader["MouseId"];
            Guid catId = (Guid)reader["CatId"];

            foreach(Cat cat in (from c in m_cats where c.Id == catId select c))
            {
                foreach(Mouse mouse in (from m in m_mice where m.Id == mouseId select m))
                {
                    cat.Chases.Add(mouse);
                    mouse.ChasedBy.Add(cat);
                }
            }

            return null;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Orm.Register<Cat>(new CatConverter());
            Orm.Register<Mouse>(new MouseConverter());

            Connection connection = new Connection(new SqlConnection(
                @"
                    Data Source=localhost\SQLEXPRESS;
                    MultipleActiveResultSets=True; 
                    Initial Catalog=stormy;
                    Integrated Security=SSPI;
                "));


            var cats = connection.Select<Cat>("select * from cat").ToList();
            var mice = connection.Select<Mouse>("select * from mouse").ToList();

            var chasing = connection.Select("select * from chasing", new ChaseConverter(cats, mice)).ToList();

            Common.List(cats);
        }
    }
}
