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

// This example shows autonumbering
// Necessary table is created by the program

namespace CatAndMouse7
{
    // Model:
    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ORM Converter:
    public class CatConverter : ISelectable<Cat>, IInsertable<Cat>, IAutoNumberable<Cat>
    {
        public Cat ApplySelect(IDataReader reader)
        {
            return new Cat()
                { 
                    Id = (int)reader["id"], 
                    Name = (string)reader["name"] 
                };
        }

        public string InsertSql()
        {
            return "insert into cats7(name) values(@name)"; 
        }

        public void ApplyInsert(Cat cat, IDbCommand command)
        {
            var par = command.CreateParameter();
            par.ParameterName = "@name";
            par.DbType = DbType.String;
            par.Value = cat.Name;
            command.Parameters.Add(par);
        }

        public void ApplyAutoNumber(Cat cat, int newId)
        {
            cat.Id = newId;
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

            // Create table (if it exists, we get a message but we ignore that)
            try
            {
                connection.Execute("create table cats7(id int identity primary key, name nvarchar(255))");
            }
            catch(Exception ex) 
            {
                if (!ex.Message.StartsWith("There is already an object named"))
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // Create new cats
            var Tom = new Cat() { Name = "Tom" };
            var Jerry = new Cat() { Name = "Jerry" };

            // Insert these cats using the mapper defined above
            connection.Insert(Tom);
            connection.Insert(Jerry);

            // And they have auto-numbered ID's
            Console.WriteLine("Tom: id={0}, Jerry: id={1}", Tom.Id, Jerry.Id);

            var cats = connection.Select<Cat>("select * from cats7");
            Console.WriteLine(String.Format("{0} cats available", cats.Count()));

        }
    }
}
