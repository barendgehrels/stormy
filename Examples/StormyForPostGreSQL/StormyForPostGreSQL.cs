using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using Stormy;

// BEFORE this program, run:
// create table cats(id serial, name varchar(255));
// insert into cats(name) values('Tom');
// insert into cats(name) values('Jerry');

namespace StormyForPostGreSQL
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
            return "insert into cats(name) values(@name)";
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


    class StormyForPostGreSQL
    {
        static void Main(string[] args)
        {
            Orm.Register<Cat>(new CatConverter());

            // Change the connectionstring below, OR create a login role "for_samples", with password "sample"
            // Followed by:
            // grant all on cats to for_samples
            // grant all on cats_id_seq to for_samples

            var connection = new Stormy.Connection(new Npgsql.NpgsqlConnection("Server=localhost;User Id=for_samples;Password=sample;Database=stormy"));

            var Tom = new Cat() { Name = "Tom" };
            var Jerry = new Cat() { Name = "Jerry" };

            // Insert these cats using the mapper defined above
            connection.Insert(Tom);
            connection.Insert(Jerry);

            // And they have auto-numbered ID's
            Console.WriteLine("Tom: id={0}, Jerry: id={1}", Tom.Id, Jerry.Id);

            var cats = connection.Select<Cat>("select * from cats");
        }
    }
}
