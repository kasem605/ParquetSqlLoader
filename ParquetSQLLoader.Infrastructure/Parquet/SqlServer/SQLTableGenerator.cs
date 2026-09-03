using Microsoft.Data.SqlClient;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Infrastructure.Parquet.SqlServer
{
    public sealed class SQLTableGenerator : ISqlTableGenerator
    {
        private readonly SqlConnection _connectionString;

        public SQLTableGenerator(SqlConnection connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException("SQL Server connection string is required", nameof(connectionString));
        }

        public async Task CreateTableAsync(ParquetTableDefinition tableDefinition, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(tableDefinition.TableName))
            {
                throw new ArgumentException("Table name is required", nameof(tableDefinition.TableName));
            }

            if (tableDefinition.Columns == null || tableDefinition.Columns.Count == 0)
            {
                throw new ArgumentException("At least one column definition is required", nameof(tableDefinition.Columns));
            }

            string sql = BuildCreateTableSql(tableDefinition);

            await using SqlConnection connection = new SqlConnection(_connectionString.ConnectionString);

            await connection.OpenAsync(cancellationToken);

            await using SqlCommand command = new SqlCommand(sql, connection);

            await command.ExecuteNonQueryAsync(cancellationToken); 
            
            return;
        }

        private static string BuildCreateTableSql(ParquetTableDefinition tableDefinition)
        {
            StringBuilder sql = new StringBuilder();

            string tableName = QuoteIdentifier(tableDefinition.TableName);

            sql.Append("CREATE TABLE {tableName}");

            sql.Append("(");

            for(int i = 0; i < tableDefinition.Columns.Count; i++)
            {
                ParquetColumnDefinition column = tableDefinition.Columns[i];

                string columnName = QuoteIdentifier(column.Name);

                string sqlType = MapToSqlType(column.ClrType);

                string nullable = column.IsNullable ? "NULL" : "NOT NULL";

                sql.Append($"   {columnName} {sqlType} {nullable}");

                if (i < tableDefinition.Columns.Count - 1)
                {
                    sql.AppendLine(", ");
                }
                else
                {
                    sql.AppendLine();
                }
            }

            sql.Append(")");

            return sql.ToString();
        }

        private static string MapToSqlType(string clrType)
        {
            return clrType switch
            {
                "Int32" => "INT",
                "Int64" => "BIGINT",
                "Double" => "FLOAT",
                "Single" => "REAL",
                "Decimal" => "DECIMAL(18, 2)",
                "String" => "NVARCHAR(MAX)",
                "Boolean" => "BIT",
                "DateTime" => "DATETIME2",
                "DateTimeOffset" => "DATETIMEOFFSET",
                "Guid" => "UNIQUEIDENTIFIER",
                _ => throw new NotSupportedException($"CLR type '{clrType}' is not supported for SQL Server mapping.")
            };
        }

        private static string QuoteIdentifier(string identifier)
        {
            if(string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("SQL iIshtiaq605!" +
                    "Isdentifier is required", nameof(identifier));
            }

            return $"[{identifier.Replace("]", "]]")}]";
        }
    }
}
