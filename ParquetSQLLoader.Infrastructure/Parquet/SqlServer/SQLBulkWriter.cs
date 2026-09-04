using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ParquetSQLLoader.Infrastructure.Parquet.SqlServer
{
    public class SQLBulkWriter : ISqlBulkWriter
    {
        private readonly string _connectionString;

        public SQLBulkWriter(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException("SQL Server connection string is required", nameof(connectionString));
        }

        public async Task WriteAsync(string tablename, RowBatch batch, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tablename, nameof(tablename));

            ArgumentNullException.ThrowIfNull(batch);

            cancellationToken.ThrowIfCancellationRequested();

            if(batch.RowCount == 0)
            {
                return;
            }

            if(batch.Columns.Count == 0)
            {
                throw new ArgumentException("RowBatch must contain at least one column", nameof(batch));
            }

            if(batch.Rows.Count != batch.RowCount)
            {
                throw new InvalidOperationException($"RowBatch.RowCount ({batch.RowCount}) does not match the number of rows in RowBatch.Rows ({batch.Rows.Count})");
            }

            ValidateRows(batch);

            await using SqlConnection connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            using SqlBulkCopy bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = QuoteTableName(tablename),
                BatchSize = batch.RowCount,

                BulkCopyTimeout = 0, // Set to 0 for no timeout
            };

            for(int columnIndex = 0; columnIndex < batch.Columns.Count; columnIndex++)
            {
                bulkCopy.ColumnMappings.Add(batch.Columns[columnIndex].Name, batch.Columns[columnIndex].Name);
            }

            using DataTable dataTable = CreateDataTable(batch);

            await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        }

        private static string QuoteTableName(string tablename)
        {
            if(tablename.Contains('.'))
            {
                string[] parts = tablename.Split('.', StringSplitOptions.RemoveEmptyEntries);
                
                if(parts.Length != 2)
                {
                    throw new ArgumentException("Invalid table name format. Expected 'schema.table'", nameof(tablename));
                }

                return $"{QuoteIdentifier(parts[0])}.{QuoteIdentifier(parts[1])}";
            }
  
            return QuoteIdentifier(tablename);
        }

        private static string QuoteIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }

        private static DataTable CreateDataTable(RowBatch batch)
        {
            DataTable dataTable = new DataTable();

            foreach (ParquetColumnDefinition column in batch.Columns)
            {
                Type clrType = GetDataTableAsync(column);

                dataTable.Columns.Add(column.Name, clrType);
            }

            foreach (IReadOnlyList<object?> row in batch.Rows)
            {
                DataRow dataRow = dataTable.NewRow();

                for (int columnIndex = 0; columnIndex < batch.Columns.Count; columnIndex++)
                {
                    object? value = row[columnIndex];

                    dataRow[columnIndex] = value ?? DBNull.Value;
                }

                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        private static Type GetDataTableAsync(ParquetColumnDefinition column)
        {
            return column.ClrType.ToLowerInvariant() switch
            {
                "string" => typeof(string),
                "int" => typeof(int),
                "int16" => typeof(short),
                "int32" => typeof(int),
                "int64" => typeof(long),
                "long" => typeof(long),
                "double" => typeof(double),
                "float" => typeof(float),
                "decimal" => typeof(decimal),
                "bool" => typeof(bool),
                "boolean" => typeof(bool),
                "datetime" => typeof(DateTime),
                "datetimeoffset" => typeof(DateTimeOffset),
                "guid" => typeof(Guid),
                "readonlymemory`1" => typeof(string),

                _ => throw new NotSupportedException($"Unsupported CLR type: {column.ClrType}"),
            };
        }

        private static void ValidateRows(RowBatch batch)
        {
            for (int rowIndex = 0; rowIndex < batch.Rows.Count; rowIndex++)
            {
                IReadOnlyList<object?> row = batch.Rows[rowIndex];

                if (row.Count != batch.Columns.Count)
                {
                    throw new InvalidOperationException($"Row {rowIndex} has {row.Count} columns, but RowBatch.Columns has {batch.Columns.Count} columns");
                }
            }

        }
    }
}
