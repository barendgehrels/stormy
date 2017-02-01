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

namespace CatAndMouse3
{
    // Method 3:
    // 1) first read all cats
    // 2) then read all mouses
    // 3) then read mouses chased by cats (COPIES!)
    // 4) then read cats chasing mouses (COPIED!)

    // This method is convenient for one-to-many relationships
    // and not for many-to-many, because of the copies.

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
        public static Mouse Convert(IDataReader reader)
        {
            Mouse mouse = new Mouse();
            mouse.Id = (Guid)reader["MouseId"];
            mouse.Name = (string)reader["name"];
            mouse.Color = (AnimalColor)reader["color"];
            return mouse;
        }

        public Mouse ApplySelect(IDataReader reader)
        {
            return Convert(reader);
        }
    }

    public class ChasesConverter : ISelectable<object>
    {
        IEnumerable<Cat> m_cats;

        public ChasesConverter(IEnumerable<Cat> cats)
        {
            m_cats = cats;
        }

        public object ApplySelect(IDataReader reader)
        {
            Guid catId = (Guid)reader["CatId"];

            // Find the cat (normally only one) (you can sort first and use Linq/binary search)
            foreach (Cat cat in (from c in m_cats where c.Id == catId select c))
            {
                // Add this mouse (reusing the mouse converter)
                cat.Chases.Add(MouseConverter.Convert(reader));

                // Because there is (normally) only one, we quit here
                return null;
            }
            return null;
        }
    }



    // Same for read chasing cats by mouses:
    // public class ChasingConverter : ISelectable<object>

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

            // Connect cats and mice.
            connection.Select("select * from chasing k join mouse m on k.MouseId=m.MouseId", new ChasesConverter(cats)).ToList();

            Common.List(cats);
        }
    }
}
