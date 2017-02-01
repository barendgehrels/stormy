using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

using Stormy;

public class Cat
{
    public string Name { get; set; }
    public double Weight { get; set; }
}

namespace StormyQuickStart
{
    class Program
    {
        const string filename = "QuickStart3.db";
    
        static void CreateDataModel(Connection connection)
        {
            connection.Execute(@"
                    create table cats(name text, weight double);
                    insert into cats values('Sockington', 5.5);
                    insert into cats values('Tom', 3.2);
                    ");
        }

        static void Main(string[] args)
        {
            bool exists = System.IO.File.Exists(filename);
            var connection = new Stormy.Connection(
                new SQLiteConnection(String.Format("Data Source={0}", filename)));
                
            // Create and fill the datamodel,
            // if the SQLite database did not exist before
            if (! exists)
            {
                CreateDataModel(connection);
            }

            var cats = connection.Select<Cat>("select * from cats",
                (reader) => new Cat()
                            {
                                Name = reader["name"].ToString(),
                                Weight = Double.Parse(reader["weight"].ToString())
                            });

            foreach (var cat in cats)
            {
                System.Console.WriteLine("Cat {0} is {1}", cat.Name, cat.Weight);
            }
        }
    }
}
