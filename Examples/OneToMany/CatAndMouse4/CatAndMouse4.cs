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

namespace CatAndMouse4
{
    public class Dummy { }


    public class AllConverter : ISelectable<Dummy>
    {
        IList<Cat> m_cats;
        IList<Mouse> m_mice;

        public AllConverter(IList<Cat> cats, IList<Mouse> mice)
        {
            m_cats = cats;
            m_mice = mice;
        }

        public Dummy ApplySelect(IDataReader reader)
        {
            Cat cat = null;
            Mouse mouse = null;

            object id = reader["CatId"];
            if (id != DBNull.Value)
            {
                Guid CatId = (Guid)id;
                // Check if cat exists, else create
                var cats = from c in m_cats where c.Id == CatId select c;
                if (cats.Count() == 0)
                {
                    cat = new Cat();
                    cat.Id = CatId;
                    cat.Name = (string)reader["name"];
                    cat.Sex = (((string)reader["sex"]) + " ")[0];
                    cat.Weight = (float)reader["weight"];
                    m_cats.Add(cat);
                }
                else
                {
                    cat = cats.First();
                }
            }

            id = reader["MouseId"];
            if (id != DBNull.Value)
            {
                Guid MouseId = (Guid)id;
                // Similar for mouse (we could make this generic as well...)
                var mice = from m in m_mice where m.Id == MouseId select m;
                if (mice.Count() == 0)
                {
                    mouse = new Mouse();
                    mouse.Id = MouseId;
                    mouse.Name = (string)reader["mouse_name"];
                    mouse.Color = (AnimalColor)reader["color"];
                    m_mice.Add(mouse);
                }
                else
                {
                    mouse = mice.First();
                }
            }

            // If there are a cat and a mouse, linke them (add them to each other)
            if (cat != null && mouse != null)
            {
                cat.Chases.Add(mouse);
                mouse.ChasedBy.Add(cat);
            }

            return null;
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

            List<Cat> cats = new List<Cat>();
            List<Mouse> mice = new List<Mouse>();

            connection.Select<Dummy>(
                @"
                    select c.*,m.MouseId,m.color,m.name as mouse_name from cat c
                          full outer join chasing k on c.CatId=k.CatId
                          full outer join mouse m on k.MouseId=m.MouseId
                ", new AllConverter(cats, mice)).ToList();

            Common.List(cats);
        }
    }
}
