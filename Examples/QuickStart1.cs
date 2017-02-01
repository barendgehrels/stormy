using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

// 0. Use stormy
using Stormy;

// 1. Create a "Model" (usually, we have one already)
public class Cat
{
    public string Name { get; set; }
    public float Weight { get; set; }
}

// 2. Create a "Mapper" 
public class CatMapper : ISelectable<Cat>
{
    public Cat ApplySelect(IDataReader reader)
    {
        return new Cat()
            {
                Name = reader["name"].ToString(),
                Weight = (float)reader["weight"]
            };
    }
}

namespace StormyQuickStart
{
    class Program
    {
        static void Main(string[] args)
        {
            // 3. Register the mapper, such that CatMapper is used for model Cat
            Orm.Register<Cat>(new CatMapper());

            // 4. Create the connection (connection strings are just ADO.NET)
            var connection = new Stormy.Connection(new SqlConnection(
                    @"
                        Data Source=localhost\SQLEXPRESS;
                        Initial Catalog=stormy;
                        Integrated Security=SSPI;
                    "));

            // 5. Get list of models (here Cats) using plain SQL statements
            foreach (var cat in connection.Select<Cat>("select * from cats"))
            {
                System.Console.WriteLine("Cat {0} is {1}", cat.Name, cat.Weight);
            }
        }
    }
}
