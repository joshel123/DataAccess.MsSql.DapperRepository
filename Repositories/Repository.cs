using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using DataAccess.MsSql.DapperRepository.Interfaces;
using DataAccess.MsSql.DapperRepository.Tools;

namespace DataAccess.MsSql.DapperRepository.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private IDbConnection Connection { get; set; }

        private readonly string _tableName;

        private readonly SqlBulkCopyForDapper<T> _bulkCopier;

        private int _timeoutInSeconds = 500;

        public Repository(IDbConnection conn)
        {
            Connection = conn;
            _tableName = GetTableName(typeof(T));
            _bulkCopier = new SqlBulkCopyForDapper<T>((SqlConnection)conn, _tableName);

        }

        public void SetTimeout(int seconds) => _timeoutInSeconds = seconds;


        /// <summary>
        /// Returns a single result by the key
        /// </summary>
        /// <param name="key">The primary key of the entity</param>
        /// <returns></returns>
        public async Task<T> Get(int key) => await Connection.GetAsync<T>(key);

        /// <summary>
        /// Returns a single result by the key
        /// </summary>
        /// <param name="key">The primary key of the entity</param>
        /// <returns></returns>
        public async Task<T> Get(long key) => await Connection.GetAsync<T>(key);


        /// <summary>
        /// Returns the entire repository.
        /// </summary>
        /// <returns>All entities in the repo</returns>
        public async Task<IEnumerable<T>> GetAll() => await Connection.GetAllAsync<T>();


        /// <summary>
        /// Returns all results returned by the command or stored proc passed in.
        /// This method allows for parameters to be passed into the proc or statement as an object 
        /// </summary>
        /// <param name="commandOrQuery">Stored proc name or query to execute</param>
        /// <param name="parameters">Object containing stored proc parameters</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetDataWithProc(string commandOrQuery, object parameters) =>
             await Connection.QueryAsync<T>(commandOrQuery, parameters, commandType: CommandType.StoredProcedure, commandTimeout: _timeoutInSeconds);

        /// <summary>
        /// Returns all results returned by the command or stored proc passed in.
        /// This method allows for parameters to be passed into the proc or statement as an object 
        /// </summary>
        /// <param name="commandOrQuery">Stored proc name or query to execute</param>
        /// <param name="parameters">Object containing stored proc parameters</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetDataWithQuery(string commandOrQuery, object parameters) =>
           await Connection.QueryAsync<T>(commandOrQuery, parameters, commandType: CommandType.Text);


        /// <summary>
        /// This version of GetData assumes that the implementing entity name is the same as the table name.
        /// It will query the table with the same name as the entity and select only those results matching the filter.
        /// 
        /// The filter attributes will be turned into a where clause.
        /// Ex {Id = 0, Name = 'Toby'} would query where Id = 0 and Name = 'Toby'
        /// </summary>
        /// The filter attributes will be turned into a where clause.
        /// <param name="filter">The object to filter results</param>
        /// <param name="table">The Table to query if different that the Entity name</param>
        /// <returns></returns>//
        
        public async Task<IEnumerable<T>> GetDataByFilter(string table = "", params KeyValuePair<string, string>[] filter)
        {
            table = string.IsNullOrEmpty(table) ? _tableName : table;

            var query = $"SELECT * FROM {table} WHERE ";
            for (int i = 0; i < filter.Length; i++)
            {
                var value = filter[i].Value;
                //replace any quotes with double quotes... sql will error otherwise
                if (value is string) value = value.Replace("'", "''");

                query = query + ($" [{filter[i].Key}] = '{filter[i].Value}' ");

                if (i != filter.Length - 1)
                    query = query + " AND ";
            }

            return await GetDataWithQuery(query.ToString(), null);
        }

        /// <summary>
        /// This version of GetData assumes that the implementing entity name is the same as the table name.
        /// It will query the table with the same name as the entity and select only those results matching the filter.
        /// 
        /// The filter attributes will be turned into a where clause.
        /// Ex {Id = 0, Name = 'Toby'} would query where Id = 0 and Name = 'Toby'
        /// </summary>
        /// The filter attributes will be turned into a where clause.
        /// <param name="filter">The object to filter results</param>
        /// <returns></returns>//
        public async Task<IEnumerable<T>> GetDataByFilter(params KeyValuePair<string, string>[] filter)
        {
            string table = _tableName;

            var query = $"SELECT * FROM {table} WHERE ";
            for (int i = 0; i < filter.Length; i++)
            {
                var value = filter[i].Value;
                //replace any quotes with double quotes... sql will error otherwise
                if (value is string) value = value.Replace("'", "''");

                query = query + ($" [{filter[i].Key}] = '{filter[i].Value}' ");

                if (i != filter.Length - 1)
                    query = query + " AND ";
            }

            return await GetDataWithQuery(query, null);
        }


        /// <summary>
        /// This version of GetData assumes that the implementing entity name is the same as the table name.
        /// It will query the table with the same name as the entity and select only those results matching the filter.
        /// 
        /// The filter attributes will be turned into a where clause.
        /// Ex {Id = 0, Name = 'Toby'} would query where Id = 0 and Name = 'Toby'
        /// </summary>
        /// The filter attributes will be turned into a where clause.
        /// <param name="filter">The object to filter results</param>
        /// <returns></returns>//
        public async Task<IEnumerable<T>> GetDataByFilter(params KeyValuePair<string, object>[] filter)
        {
            string table = _tableName;

            var query = $"SELECT * FROM {table} WHERE ";
            for (int i = 0; i < filter.Length; i++)
            {
                var value = filter[i].Value;
                //replace any quotes with double quotes... sql will error otherwise
                if (value is string) value = value.ToString().Replace("'", "''");

                query = query + ($" [{filter[i].Key}] = '{filter[i].Value}' ");

                if (i != filter.Length - 1)
                    query = query + " AND ";
            }

            return await GetDataWithQuery(query, null);
        }

        /// <summary>
        /// This version of GetData assumes that the implamenting entity name is the same as the table name.
        /// It will query the table with the same name as the entity and select only those results matching the filter.
        /// 
        /// The filter attributes will be turned into a where clause.
        /// Ex {Id = 0, Name = 'Toby'} would query where Id = 0 and Name = 'Toby'
        /// </summary>
        /// The filter attributes will be turned into a where clause.
        /// <param name="table">The Table to query if different that the Entity name</param>
        /// <param name="filter">The object to filter results</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetDataByFilter(object filter, string table = "")
        {
            PropertyInfo[] propertyInfos = filter.GetType().GetProperties();
            table = string.IsNullOrEmpty(table) ? _tableName : table;

            var query = $"SELECT * FROM {table} WHERE ";
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                //replace any quotes with double quotes... sql will error otherwise
                var value = propertyInfos[i].GetValue(filter);
                if (value is string) value = value.ToString().Replace("'", "''");

                query = query + ($" [{propertyInfos[i].Name}] = '{value}' ");

                if (i != propertyInfos.Length - 1)
                    query = query + " AND ";
            }

            return await GetDataWithQuery(query, filter);
        }



        public async Task<IEnumerable<T>> GetAllMatchingDataByFilter(string table = "", params KeyValuePair<string, string>[] filter)
        {
            table = string.IsNullOrEmpty(table) ? _tableName : table;

            var query = $"SELECT * FROM {table} WHERE ";
            for (int i = 0; i < filter.Length; i++)
            {
                var value = filter[i].Value;
                //replace any quotes with double quotes... sql will error otherwise
                if (value is string) value = value.Replace("'", "''");

                query = query + ($" [{filter[i].Key}] like '%{filter[i].Value}%' ");

                if (i != filter.Length - 1)
                    query = query + " OR ";
            }

            return await GetDataWithQuery(query.ToString(), null);
        }

        /// <summary>
        /// Will insert entity into table
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <returns></returns>
        public async Task<long> Insert(T entity) => await Connection.InsertAsync(entity);

        /// <summary>
        /// Will insert list of entity into table
        /// </summary>
        /// <param name="entities">List to insert</param>
        /// <returns></returns>
        public async Task<long> Insert(IEnumerable<T> entities) => await Connection.InsertAsync(entities);

        /// <summary>
        /// Will delete entity from table
        /// </summary>
        /// <param name="entities">List to insert</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Delete(T entity) => await Connection.DeleteAsync(entity);

        /// <summary>
        /// Will delete list of entity from table
        /// </summary>
        /// <param name="entities">List to insert</param>
        /// <returns></returns>

        public async Task<bool> Delete(IEnumerable<T> entities) => await Connection.DeleteAsync(entities);


        /// <summary>
        /// Will update entity in table
        /// </summary>
        /// <param name="entities">List to insert</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Update(T entity) => await Connection.UpdateAsync(entity);


        /// <summary>
        /// Will update list of entities in table
        /// </summary>
        /// <param name="entities">List to insert</param>
        /// <returns></returns>
        public async Task<bool> Update(IEnumerable<T> entities) => await Connection.UpdateAsync(entities);

        /// <summary>
        /// Executes the given procedure
        /// </summary>
        /// <param name="proc">The name of the proc to execute</param>
        /// <param name="parameters">Parameters to pass into proc</param>
        public async Task ExecuteProc(string proc, object parameters) =>
            await Connection.ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure, commandTimeout: _timeoutInSeconds);

        /// <summary>
        /// Executes the given statement
        /// </summary>
        /// <param name="statement">The query to execute as a string</param>
        /// <param name="parameters">Parameters to pass into statement</param>
        public async Task ExecuteStatement(string statement, object parameters) =>
            await Connection.ExecuteAsync(statement, parameters, commandType: CommandType.StoredProcedure, commandTimeout : _timeoutInSeconds);

        public async Task<long> BulkInsert(IEnumerable<T> entities) => await _bulkCopier.BulkCopy(entities);

        private string GetTableName(Type type)
        {
            var table = GetTableAttributeValue(typeof(T));
            return string.IsNullOrEmpty(table) ? typeof(T).Name : table;
        }

        private string GetTableAttributeValue(Type type)
        {

            dynamic tableattr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute");
            var name = string.Empty;

            if (tableattr != null)
                name = tableattr.Name;

            return name;
        }



    }
}
