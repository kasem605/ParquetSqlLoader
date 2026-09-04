using Parquet;
using Parquet.Schema;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using System.Runtime.CompilerServices;

namespace ParquetSQLLoader.Infrastructure.Parquet
{
    public sealed class ParquetRowReader : IParquetRowReader
    {
        public async IAsyncEnumerable<RowBatch> ReadRowsAsync(string filePath,[EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Open Parquet file

            if(string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Parquet file path is required.", nameof(filePath));
            }

            if(!File.Exists(filePath)) 
            {
                throw new FileNotFoundException("Parquet file not found.", filePath);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await using Stream fileStream = File.OpenRead(filePath);

            await using ParquetReader parquetReader = await ParquetReader.CreateAsync(fileStream, cancellationToken: cancellationToken);

            DataField[]? dataFields = parquetReader.Schema.GetDataFields();

            IReadOnlyList<ParquetColumnDefinition> columns = parquetReader.Schema.GetDataFields().Select(f => new ParquetColumnDefinition
            {
                Name = f.Name,
                ClrType = GetApplicationClrType(f.ClrType),
                IsNullable = f.IsNullable
            }).ToList();

            for(int rowGroupIndex = 0; rowGroupIndex < parquetReader.RowGroups.Count; rowGroupIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using ParquetRowGroupReader rowGroupReader = parquetReader.OpenRowGroupReader(rowGroupIndex);

                int rowCount = checked((int)rowGroupReader.RowCount);

                // Read each column

                var columnData = new object?[dataFields.Length];

                for(int columnIndex = 0; columnIndex < dataFields.Length;columnIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    columnData[columnIndex] = await ReadColumnAsync(rowGroupReader, dataFields[columnIndex], rowCount, cancellationToken);

                }

                // Convert column-oriented Parquet data
                // into your row-oriented RowBatch

                List<IReadOnlyList<object?>> rows = new List<IReadOnlyList<object?>>(rowCount);

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {

                    object?[]? row = new object?[dataFields.Length];

                    for (int columnIndex = 0; columnIndex < dataFields.Length; columnIndex++)
                    {
                        Array values = (Array)columnData[columnIndex]!;

                        row[columnIndex] = values.GetValue(rowIndex);
                    }

                    rows.Add(row);
                }

                yield return new RowBatch
                {
                    RowCount = rowCount,
                    Columns = columns,
                    Rows = rows
                };
            }

        }

        private static string GetApplicationClrType(Type clrType)
        {
            if(clrType == typeof(ReadOnlyMemory<byte>))
            {
                return "String";
            }

            return clrType.Name;
        }

        private static async Task<Array> ReadColumnAsync(ParquetRowGroupReader rowGroupReader, DataField dataField, int rowCount, CancellationToken cancellationToken)
        {
            System.Type clrType = dataField.ClrType;

            // STRING
            if(clrType == typeof(ReadOnlyMemory<byte>) || clrType == typeof(ReadOnlyMemory<char>))
            {
                string?[]? values = new string?[rowCount];

                await rowGroupReader.ReadAsync(dataField, values);

                return values;
            }

            // INT64
            if (clrType == typeof(long))
            {
                long?[]? values = new long?[rowCount];

                await rowGroupReader.ReadAsync(dataField, values.AsMemory(), cancellationToken: cancellationToken);

                return values;
            }

            // DOUBLE
            if (clrType == typeof(double))
            {
                double?[]? values = new double?[rowCount];

                await rowGroupReader.ReadAsync(dataField, values.AsMemory(), cancellationToken: cancellationToken);

                return values;
            }

            // BOOLEAN
            if (clrType == typeof(bool))
            {
                bool?[]? values = new bool?[rowCount];

                await rowGroupReader.ReadAsync(dataField, values.AsMemory(), cancellationToken: cancellationToken);

                return values;
            }

            // Handle other types as needed

            throw new NotSupportedException($"Parquet CLR type '{clrType.FullName}' for column {dataField.Name} is not currently supported.");
        }

    }
}
