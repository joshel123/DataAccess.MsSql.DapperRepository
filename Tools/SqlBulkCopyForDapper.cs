using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.MsSql.DapperRepository.Tools
{
    public class SqlBulkCopyForDapper<T> where T : class
    {
        public int CommitBatchSize { get; set; } = 1000;

        public int TimeoutInSeconds { get; set; } = 120;

        private readonly string _tableName;

        private bool IsTableMapped { get; set; }

        private List<SqlBulkCopyColumnMapping> TableMapping { get; set; }

        private SqlConnection Connection { get; set; }


        public SqlBulkCopyForDapper(SqlConnection connection, string tableName)
        {
            _tableName = tableName;
            IsTableMapped = false;
            Connection = connection;
            TableMapping = new List<SqlBulkCopyColumnMapping>();
        }

        private void MapTable()
        {

        }

        public async Task<long> BulkCopy(IEnumerable<T> objToInsert)
        {
            var entityList = objToInsert.ToList();
            var taskList = new List<Task<long>>();

            if (entityList.Any())
            {
                taskList.Add(DataTransferTask(entityList.ToDataTable()));

                await Task.WhenAll(taskList);
            }

            return taskList.Sum(x => x.Result);
        }

        private async Task<long> DataTransferTask(DataTable dt)
        {
            long result = 0;

            if (!IsTableMapped)
                MapObjectToTable(dt);

            var copyOptions = SqlBulkCopyOptions.FireTriggers |
                              SqlBulkCopyOptions.UseInternalTransaction;

            using (SqlBulkCopy bk = new SqlBulkCopy(Connection, copyOptions, null)
            {
                DestinationTableName = _tableName,
                BulkCopyTimeout = TimeoutInSeconds,
                BatchSize = CommitBatchSize
            })
            {
                foreach (var map in TableMapping)
                    bk.ColumnMappings.Add(map);

                //Connection closes because this is in batches
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                await bk.WriteToServerAsync(dt);
                result = bk.TryGetRowsCopied(out int rowsCopied) ? rowsCopied : 0;

                bk.Close();
            }


            return result;
        }

        private void MapObjectToTable(DataTable dt)
        {
            foreach (DataColumn col in dt.Columns)
            {
                TableMapping.Add(new SqlBulkCopyColumnMapping
                {
                    DestinationColumn = col.ColumnName,
                    SourceColumn = col.ColumnName
                });
            }

            IsTableMapped = true;
        }
    }
}
