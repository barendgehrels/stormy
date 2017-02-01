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

/* 
 * Prepare: create database Stormy and execute:
create table cat(CatId uniqueidentifier primary key, name nvarchar(16), sex nvarchar(1), weight real);
insert into cat(CatId, name, sex, weight) values(NEWID(), 'Sockington', 'm', 5.5);
insert into cat(CatId, name, sex, weight) values(NEWID(), 'Scarlett', 'f', 5.0);
 * 
 */

namespace StormyCat
{
    // Our "Model"
    public class Cat
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public char Sex { get; set; }
        public float Weight { get; set; }
    }

    // Implement ORM Conversion:
    public class CatConverter : ISelectable<Cat>, IInsertable<Cat>, IDeleteable<Cat>
    {
        public Cat ApplySelect(IDataReader reader)
        {
            // This below is quite inconvenient - but commonly found in samples.
            // It might be enhanced - using specialization again
            // e.g. cat.Weight = reader.As<float>("sex");

            Cat cat = new Cat();
            cat.Id = (Guid)reader["CatId"];
            cat.Name = (string)reader["name"];
            cat.Sex = (((string)reader["sex"]) + " ")[0];
            cat.Weight = (float)reader["weight"];
            return cat;
        }

        // IInsertable part
        public void ApplyInsert(Cat cat, IDbCommand command)
        {
            command.Parameters.Add(new SqlParameter("@name", cat.Name));
            command.Parameters.Add(new SqlParameter("@sex", cat.Sex));
            command.Parameters.Add(new SqlParameter("@weight", cat.Weight));
        }

        public string InsertSql()
        {
            return "insert into Cat(CatId, Name, Sex, Weight) values(NEWID(), @name, @sex, @weight)";
        }

        // IDeletable part
        public void ApplyDelete(Cat cat, IDbCommand command)
        {
            command.Parameters.Add(new SqlParameter("@id", cat.Id));
        }

        public string DeleteSql()
        {
            return "delete from Cat where CatId=@id";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Orm.Register<Cat>(new CatConverter());

            Connection connection = new Connection(new SqlConnection(
                @"
                    Data Source=localhost\SQLEXPRESS;
                    MultipleActiveResultSets=True; 
                    Initial Catalog=stormy;
                    Integrated Security=SSPI;
                "));


            var cats = connection.Select<Cat>("select * from cat").ToList();
            foreach(Cat cat in cats)
            {
                System.Console.WriteLine("Cat {0} named {1}", cat.Id, cat.Name);
            }

            // Create subselection using linq (not related to SpecializationbyTraits)
            var toms = from cat in cats where cat.Name == "Tom" select cat;
            if (toms.Any())
            {
                Cat tom = toms.First();
                System.Console.WriteLine("Tom found, ID={0}", tom.Id);
                connection.Delete(tom);
            }
            else
            {
                Cat tom = new Cat();
                tom.Name = "Tom";
                tom.Sex = 'm';
                tom.Weight = 5.2F;
                connection.Insert(tom);
            }
        }
    }
}
