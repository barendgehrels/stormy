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


namespace CatAndMouse2
{

    // ORM Conversions:
    public class CatConverter : ISelectable<Cat>
    {
        private Cat lastCat = null;

        public Cat ApplySelect(IDataReader reader)
        {
            Guid id = (Guid)reader["CatId"];
            bool newCat = lastCat == null || lastCat.Id != id;
            Cat cat = newCat ? null : lastCat;
            if (newCat)
            {
                cat = new Cat();
                cat.Id = id;
                cat.Name = (string)reader["name"];
                cat.Sex = (((string)reader["sex"]) + " ")[0];
                cat.Weight = (float)reader["weight"];
                lastCat = cat;
            }

            if (reader["MouseId"] != DBNull.Value)
            {
                // Add new mouse to new (or existing) cat
                Mouse mouse = new Mouse();
                mouse.Id = (Guid)reader["MouseId"];
                mouse.Name = (string)reader["MouseName"];
                mouse.Color = (AnimalColor)reader["MouseColor"];

                cat.Chases.Add(mouse);
            }
            return newCat ? cat : null;
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

            var cats = connection.Select<Cat>(
                @"
                    select c.*,m.MouseId,m.name as MouseName,m.color as MouseColor from cat c
                        left join chasing k on c.CatId=k.CatId
                        left join mouse m on k.MouseId=m.MouseId
                ", new CatConverter()).ToList();

            Common.List(cats);
        }
    }
}
