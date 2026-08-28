using ParquetSQLLoader.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ParquetSQLLoader.SqlServer
{
    public sealed class SqlBulkWriter : ISqlBulkWriter
    {
        private readonly string _connectionString;

        public SqlBulkWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task WriteAsync(string tablename, IDataReader reader, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
