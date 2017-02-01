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
create table owner(OwnerId uniqueidentifier primary key, name nvarchar(16));
insert into owner values(NEWID(), 'Barend'), (NEWID(), 'Rob')

create table cat(CatId uniqueidentifier primary key, name nvarchar(16), sex nvarchar(1), weight real, OwnerId uniqueidentifier);
alter table cat
  add constraint FK_OwnerId foreign key (OwnerId) references owner(OwnerId);
insert into cat values
(NEWID(), 'Tom', 'm', 5.2, (select OwnerId from owner where name = 'Rob')), 
(NEWID(), 'Sylvester', 'm', 5.3, null), 
(NEWID(), 'Scarlett', 'm', 5, (select OwnerId from owner where name = 'Barend')), 
(NEWID(), 'Sockington', 'm', 5.5, (select OwnerId from owner where name = 'Barend'))
 */


namespace CatAndMouse6
{

    // Stubb class to create converters returning nothing.
    // Note that they might use "object" or even "int" as well
    // This underscore might be considered as a bit too subtile...
    public class _ { }


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

    public class OwnerConverter : ISelectable<Owner>
    {
        public Owner ApplySelect(IDataReader reader)
        {
            Owner owner = new Owner();
            owner.Id = (Guid)reader["OwnerId"];
            owner.Name = (string)reader["name"];

            return owner;
        }
    }

    public class OwnedByConverter : ISelectable<_>
    {
        IEnumerable<Cat> cats;
        IEnumerable<Owner> owners;

        public OwnedByConverter(IEnumerable<Cat> cats, IEnumerable<Owner> owners)
        {
            this.cats = cats;
            this.owners = owners;
        }

        public _ ApplySelect(IDataReader reader)
        {
            Cat ownedCat = null;
            Guid CatId = (Guid)reader["CatId"];
            foreach (Cat cat in this.cats)
            {
                if (cat.Id == CatId)
                {
                    ownedCat = cat;
                    break;
                }
            }

            Owner catIsOwnedBy = null;
            if (reader["OwnerId"] != DBNull.Value)
            {
                Guid OwnerId = (Guid)reader["OwnerId"];

                foreach (Owner owner in this.owners)
                {
                    if (owner.Id == OwnerId)
                    {
                        catIsOwnedBy = owner;
                        break;
                    }
                }
                ownedCat.IsOwnedBy = catIsOwnedBy;
                catIsOwnedBy.Cats.Add(ownedCat);
            }

            return null;
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
            Orm.Register<Owner>(new OwnerConverter());
            Orm.Register<Cat>(new CatConverter());

            Connection connection = new Connection(new SqlConnection(
                @"
                    Data Source=localhost\SQLEXPRESS;
                    MultipleActiveResultSets=True; 
                    Initial Catalog=stormy;
                    Integrated Security=SSPI;
                "));

            var cats = connection.Select<Cat>("select * from cat2").ToList();
            var owners = connection.Select<Owner>("select * from owner2").ToList();

            // connect cats with their owners
            connection.Select<_>(
                @"
                    select c.CatId, o.OwnerId from cat2 c left join owner2 o on c.OwnerId=o.OwnerId
                ", new OwnedByConverter(cats, owners)).ToList();

            foreach (Cat cat in cats)
            {
                System.Console.WriteLine("Cat {0} named {1}", cat.Id, cat.Name);
                // TEMP - to indicate the owner owns cats by reference
                // cat.Name = cat.Name + " renamed";
            }
            foreach (Owner owner in owners)
            {
                System.Console.WriteLine("Owner {0} named {1}", owner.Id, owner.Name);
                foreach (Cat cat in owner.Cats)
                {
                    System.Console.WriteLine(" - Owns cat {0} named {1}", cat.Id, cat.Name);
                }
            }
        }
    }
}
