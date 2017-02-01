using System;
using System.Collections.Generic;

// 0. Use the SqlClient; SqlServer types, and Stormy
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using Stormy;


// 1. Create a "Model" (usually, we have one already)
public class City
{
    public string Name { get; set; }
    public SqlGeometry Location { get; set; }
}

// 2. Create a "Mapper" 
public class CityMapper : ISelectable<City>
{
    public City ApplySelect(IDataReader reader)
    {
        return new City()
            {
                Name = reader["name"].ToString(),
                Location = (SqlGeometry)reader["location"]
            };
    }
}


namespace StormyGeometry
{
    class Program
    {
        static void Main(string[] args)
        {
            // 3. Register the mapper, such that it is used for model City
            Orm.Register<City>(new CityMapper());

            // 4. Create the connection (connection strings are just ADO.NET)
            var connection = new Stormy.Connection(new SqlConnection(
                    @"
                        Data Source=localhost\SQLEXPRESS;
                        Initial Catalog=stormy;
                        Integrated Security=SSPI;
                    "));

            // 5. Get list of models (here Cities) using SQL 
            var cities = connection.Select<City>("select * from cities");
            foreach (City city in cities)
            {
                // Just use the SqlGeometry methods such as STX, STY, STIntersects, etc.
                System.Console.WriteLine("City {0} location lat={1}, lon={2}", 
                    city.Name, city.Location.STY, city.Location.STX);
            }
        }
    }
}
