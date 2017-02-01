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
using System.Data;

// SOCI-like ORM based on C# and Specialization by Traits

// Version 2, July 29, 2010

// SQLite version, October 22, 2011
// Using SQLites data provider described here: 
// http://www.mikeduncan.com/sqlite-on-dotnet-in-3-mins/
// The actual and updated provider is now here:
// http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki
// and works awesome.

// Version 3, November 30, 2011
// Supporting auto-numbering

// Version 4, January 1, 2012
// One version for SQLServer, SQLite and PostGreSQL.
// Reworked ISelectable.
// Cleaned up.

namespace Stormy
{
    // Originally based on SOCI soci::type_conversion<T> specialization

    public interface IConvertable<T>
    {
    }

    public interface ISelectable<T> : IConvertable<T>
    {
        T ApplySelect(IDataReader reader);
    }

    public interface IInsertable<T> : IConvertable<T>
    {
        void ApplyInsert(T obj, IDbCommand command);
        string InsertSql();
    }

    public interface IAutoNumberable<T> : IConvertable<T>
    {
        void ApplyAutoNumber(T obj, int newId);
    }

    public interface IUpdateable<T> : IConvertable<T>
    {
        void ApplyUpdate(T obj, IDbCommand command);
        string UpdateSql();
    }

    public interface IDeleteable<T> : IConvertable<T>
    {
        void ApplyDelete(T obj, IDbCommand command);
        string DeleteSql();
    }

    public static class Orm
    {
        private static IDictionary<System.Type, object> m_select_register;
        private static IDictionary<System.Type, object> m_insert_register;
        private static IDictionary<System.Type, object> m_delete_register;
        private static IDictionary<System.Type, object> m_update_register;
        private static IDictionary<System.Type, object> m_autonumber_register;

        private static void Init()
        {
            if (m_select_register == null)
            {
                m_select_register = new Dictionary<System.Type, object>();
                m_insert_register = new Dictionary<System.Type, object>();
                m_delete_register = new Dictionary<System.Type, object>();
                m_update_register = new Dictionary<System.Type, object>();
                m_autonumber_register = new Dictionary<System.Type, object>();
            }
        }

        public static void RegisterSelect<T>(ISelectable<T> converter)
        {
            Init();
            m_select_register[typeof(T)] = converter;
        }

        public static void RegisterInsert<T>(IInsertable<T> converter)
        {
            Init();
            m_insert_register[typeof(T)] = converter;
        }

        public static void RegisterDelete<T>(IDeleteable<T> converter)
        {
            Init();
            m_delete_register[typeof(T)] = converter;
        }

        // Generic register function, in case the converter implements
        // all interfaces.
        public static void Register<T>(IConvertable<T> converter)
        {
            Init();
            if (converter is ISelectable<T>)
            {
                m_select_register[typeof(T)] = converter;
            }
            if (converter is IInsertable<T>)
            {
                m_insert_register[typeof(T)] = converter;
            }
            if (converter is IAutoNumberable<T>)
            {
                m_autonumber_register[typeof(T)] = converter;
            }
            if (converter is IUpdateable<T>)
            {
                m_update_register[typeof(T)] = converter;
            }
            if (converter is IDeleteable<T>)
            {
                m_delete_register[typeof(T)] = converter;
            }
        }

        public static ISelectable<T> GetSelectable<T>()
        {
            return m_select_register[typeof(T)] as ISelectable<T>;
        }
        public static IInsertable<T> GetInsertable<T>()
        {
            return m_insert_register[typeof(T)] as IInsertable<T>;
        }
        public static IAutoNumberable<T> GetAutoNumberable<T>()
        {
            return m_autonumber_register.ContainsKey(typeof(T)) ? m_autonumber_register[typeof(T)] as IAutoNumberable<T> : null;
        }
        public static IUpdateable<T> GetUpdateable<T>()
        {
            return m_update_register[typeof(T)] as IUpdateable<T>;
        }
        public static IDeleteable<T> GetDeleteable<T>()
        {
            return m_delete_register[typeof(T)] as IDeleteable<T>;
        }
    }

    /// <summary>
    /// Specific parameter to function as IDbParameter. First versions used IDbParameter but SQL Server cannot
    /// create parameters from one command and use them in another. So we copy them now, and then it is more convenient
    /// to have our own class
    /// </summary>
    public class StormyParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DbType Type { get; set; }
    }

    public class Connection
    {
        private IDbConnection m_connection;

        public delegate T DelegateReader<T>(IDataReader readMapper);
        public delegate bool DelegatePredicate<T>(T input);

        public Connection(IDbConnection connection)
        {
            m_connection = connection;
            if (m_connection.State != ConnectionState.Open)
            {
                m_connection.Open();
            }
        }

        private IDbDataParameter CreateParameter(IDbCommand command, StormyParameter source)
        {
            var result = command.CreateParameter();
            result.DbType = source.Type;
            result.ParameterName = source.Name;
            result.Value = source.Value;
            return result;
        }

        private void TranslateParameters(IDbCommand command, IEnumerable<StormyParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var par in parameters)
                {
                    command.Parameters.Add(CreateParameter(command, par));
                }
            }
        }

        private string GetLastInsertedAutoNumberStatement()
        {
            // This is currently the only database-specific dependancy
            switch (m_connection.GetType().Name.ToUpper().Replace("CONNECTION", ""))
            {
                case "SQLITE": return "select last_insert_rowid()";
                case "SQL": return "select @@identity";
                case "NPGSQL": return "select lastval()";
            }
            throw new Exception("Unknown autonumber statement");
        }

        private int GetLastInsertRowId()
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = GetLastInsertedAutoNumberStatement();               
                using (IDataReader reader = command.ExecuteReader())
                {
                    return reader.Read() ? Int32.Parse(reader[0].ToString()) : 0;
                }
            }
        }

        public IEnumerable<T> Select<T>(String statement, ISelectable<T> traits, IEnumerable<StormyParameter> parameters)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                TranslateParameters(command, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T obj = traits.ApplySelect(reader);
                        if (obj != null)
                        {
                            yield return obj;
                        }
                    }
                }
            }
        }

        public IEnumerable<T> Select<T>(String statement, ISelectable<T> traits)
        {
            return Select<T>(statement, traits, null);
        }

        // Version using a delegate (for one-time mappers)
        public IEnumerable<T> Select<T>(String statement, DelegateReader<T> readMapper)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return readMapper(reader);
                    }
                }
            }
        }

        public IEnumerable<T> Select<T>(String statement)
        {
            return Select(statement, Orm.GetSelectable<T>());
        }

        public IEnumerable<T> Select<T>(String statement, IEnumerable<StormyParameter> parameters)
        {
            return Select(statement, Orm.GetSelectable<T>(), parameters);
        }

        // ---------------------------------------------------------------------
        // Insert
        // ---------------------------------------------------------------------
        public void Insert<T>(T model, String statement, IInsertable<T> traits)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                traits.ApplyInsert(model, command);
                command.ExecuteNonQuery();
            }

            IAutoNumberable<T> anTraits
                = traits is IAutoNumberable<T>
                ? traits as IAutoNumberable<T>
                : Orm.GetAutoNumberable<T>()
                ;

            if (anTraits != null)
            {
                anTraits.ApplyAutoNumber(model, GetLastInsertRowId());
            }
        }

        public void Insert<T>(T model, IInsertable<T> traits)
        {
            Insert(model, traits.InsertSql(), traits);
        }

        public void Insert<T>(T model)
        {
            Insert(model, Orm.GetInsertable<T>());
        }

        public void Insert<T>(T model, String statement)
        {
            Insert(model, statement, Orm.GetInsertable<T>());
        }

        // ---------------------------------------------------------------------
        // Delete
        // ---------------------------------------------------------------------
        public void Delete<T>(T model, String statement, IDeleteable<T> traits)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                traits.ApplyDelete(model, command);
                command.ExecuteNonQuery();
            }
        }

        public void Delete<T>(T model, IDeleteable<T> traits)
        {
            Delete(model, traits.DeleteSql(), traits);
        }

        public void Delete<T>(T model, String statement)
        {
            Delete(model, statement, Orm.GetDeleteable<T>());
        }

        public void Delete<T>(T model)
        {
            Delete(model, Orm.GetDeleteable<T>());
        }

        // ---------------------------------------------------------------------
        // Update
        // ---------------------------------------------------------------------
        public void Update<T>(T model, String statement, IUpdateable<T> traits)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                traits.ApplyUpdate(model, command);
                command.ExecuteNonQuery();
            }
        }

        public void Update<T>(T model, IUpdateable<T> traits)
        {
            Update(model, traits.UpdateSql(), traits);
        }

        public void Update<T>(T model, String statement)
        {
            Update(model, statement, Orm.GetUpdateable<T>());
        }

        public void Update<T>(T model)
        {
            Update(model, Orm.GetUpdateable<T>());
        }

        // ---------------------------------------------------------------------
        // Any statement.
        // ---------------------------------------------------------------------
        public void Execute(String statement, IEnumerable<StormyParameter> parameters)
        {
            using (IDbCommand command = m_connection.CreateCommand())
            {
                command.CommandText = statement;
                TranslateParameters(command, parameters);
                command.ExecuteNonQuery();
            }
        }

        public void Execute(String statement)
        {
            Execute(statement, null);
        }

        /// <summary>
        /// Generic method inserting/updating items from first list, and deleting parameters from second list
        /// </summary>
        public void ApplyUpdates<T>(IEnumerable<T> items,
                        IList<T> deletedItems,
                        DelegatePredicate<T> insertCriterium,
                        DelegatePredicate<T> updateCriterium)
        {
            // Applying deletes
            if (deletedItems != null)
            {
                foreach (T item in deletedItems)
                {
                    this.Delete(item);
                }
                deletedItems.Clear();
            }

            // Applying inserts and updates
            if (items != null)
            {
                foreach (T item in items)
                {
                    if (insertCriterium(item))
                    {
                        this.Insert(item);
                    }
                    else if (updateCriterium(item))
                    {
                        this.Update(item);
                    }
                }
            }
        }
    }

}
