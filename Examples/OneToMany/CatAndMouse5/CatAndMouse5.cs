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

namespace CatAndMouse5
{

    // ORM Conversions:
    public class CatConverter : ISelectable<Cat>
    {
        Connection m_connection;
        public CatConverter(Connection connection)
        {
            m_connection = connection;
        }

        public Cat ApplySelect(IDataReader reader)
        {
            Cat cat = new Cat();
            cat.Id = (Guid)reader["CatId"];
            cat.Name = (string)reader["name"];
            cat.Sex = (((string)reader["sex"]) + " ")[0];
            cat.Weight = (float)reader["weight"];

            foreach (Mouse mouse in m_connection.Select<Mouse>(String.Format(
                @"
                    select * from mouse where MouseId in
                        (select MouseId from chasing where CatId='{0}')
                ", cat.Id)))
            {
                cat.Chases.Add(mouse);
            }
               
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


    class Program
    {
        static void Main(string[] args)
        {
            Connection connection = new Connection(new SqlConnection(
                @"
                    Data Source=localhost\SQLEXPRESS;
                    MultipleActiveResultSets=True; 
                    Initial Catalog=stormy;
                    Integrated Security=SSPI;
                "));

            Orm.Register<Cat>(new CatConverter(connection));
            Orm.Register<Mouse>(new MouseConverter());

            var cats = connection.Select<Cat>("select * from cat");
            var mice = connection.Select<Mouse>("select * from mouse");

            Common.List(cats, mice);
        }
    }
}
