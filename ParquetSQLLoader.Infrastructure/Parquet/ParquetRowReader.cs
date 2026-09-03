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
                ClrType = f.ClrType.Name,
                IsNullable = f.IsNullable
            }).ToList();

            for(int rowGroupIndex = 0; rowGroupIndex < parquetReader.RowGroups.Count; rowGroupIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using ParquetRowGroupReader rowGroupReader = parquetReader.OpenRowGroupReader(rowGroupIndex);

                int rowCount = checked((int)rowGroupReader.RowCount);

                // Read each column

                List<Array>? columnData = new List<Array>(dataFields.Length);

                foreach (DataField dataField in dataFields)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Array values = await ReadColumnAsync(rowGroupReader, dataField, rowCount, cancellationToken);

                    columnData.Add(values);
                }

                // Convert column-oriented Parquet data
                // into your row-oriented RowBatch

                List<IReadOnlyList<object?>> rows = new List<IReadOnlyList<object?>>(rowCount);

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    object?[]? row = new object?[dataFields.Length];

                    for (int columnIndex = 0; columnIndex < dataFields.Length; columnIndex++)
                    {
                        row[columnIndex] = columnData[columnIndex].GetValue(rowIndex);
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

        private async Task<Array> ReadColumnAsync(ParquetRowGroupReader rowGroupReader, DataField dataField, int rowCount, CancellationToken cancellationToken)
        {
            System.Type clrType = dataField.ClrType;

            // Nullable value type
            if(dataField.IsNullable && clrType.IsValueType && Nullable.GetUnderlyingType(clrType) == null)
            {
                System.Type nullableType = typeof(Nullable<>).MakeGenericType(clrType);

                Array values = Array.CreateInstance(nullableType, rowCount);

                await ReadNullableColumnAsync(rowGroupReader, dataField, values, clrType, cancellationToken);

                return values;
            }

            // Reference types and non-nullable value types

            Array buffer = Array.CreateInstance(clrType, rowCount);

            await ReadNonNullableColumnAsync(rowGroupReader, dataField, buffer, clrType, cancellationToken);

            return buffer;
        }

        private static async Task ReadNonNullableColumnAsync(ParquetRowGroupReader rowGroupReader, DataField dataField, Array buffer, System.Type clrType, CancellationToken cancellationToken)
        {
            // Parquet.Net's ReadAsync<T> is strongly typed.
            // Dispatch based on the actual CLR type.

            if (clrType == typeof(string))
            {
                await rowGroupReader.ReadAsync(dataField, (string[])buffer);
                return;
            }

            if (clrType == typeof(int))
            {
                await rowGroupReader.ReadAsync<int>(dataField, (int[])buffer);
                return;
            }

            if (clrType == typeof(long))
            {
                await rowGroupReader.ReadAsync<long>(dataField, (long[])buffer);
                return;
            }

            if (clrType == typeof(short))
            {
                await rowGroupReader.ReadAsync<short>(dataField, (short[])buffer);
                return;
            }

            if (clrType == typeof(byte))
            {
                await rowGroupReader.ReadAsync<byte>(dataField, (byte[])buffer);
                return;
            }

            if (clrType == typeof(float))
            {
                await rowGroupReader.ReadAsync<float>(dataField, (float[])buffer);
                return;
            }

            if (clrType == typeof(double))
            {
                await rowGroupReader.ReadAsync<double>(dataField, (double[])buffer);
                return;
            }

            if (clrType == typeof(bool))
            {
                await rowGroupReader.ReadAsync<bool>(dataField, (bool[])buffer);
                return;
            }

            if (clrType == typeof(decimal))
            {
                await rowGroupReader.ReadAsync<decimal>(dataField, (decimal[])buffer);
                return;
            }

            if (clrType == typeof(DateTime))
            {
                await rowGroupReader.ReadAsync<DateTime>(dataField, (DateTime[])buffer);
                return;
            }

            throw new NotSupportedException($"Parquet CLR type '{clrType.FullName}' for column {clrType.Name} is not supported.");

        }

        private async Task ReadNullableColumnAsync(ParquetRowGroupReader rowGroupReader, DataField dataField, Array buffer, System.Type clrType, CancellationToken cancellationToken)
        {
            if (clrType == typeof(string))
            {
                await rowGroupReader.ReadAsync(dataField, (string[])buffer);
                return;
            }

            if (clrType == typeof(int))
            {
                await rowGroupReader.ReadAsync<int>(dataField, (int[])buffer);
                return;
            }

            if (clrType == typeof(long))
            {
                await rowGroupReader.ReadAsync<long>(dataField, (long[])buffer);
                return;
            }

            if (clrType == typeof(short))
            {
                await rowGroupReader.ReadAsync<short>(dataField, (short[])buffer);
                return;
            }

            if (clrType == typeof(byte))
            {
                await rowGroupReader.ReadAsync<byte>(dataField, (byte[])buffer);
                return;
            }

            if (clrType == typeof(float))
            {
                await rowGroupReader.ReadAsync<float>(dataField, (float[])buffer);
                return;
            }

            if (clrType == typeof(double))
            {
                await rowGroupReader.ReadAsync<double>(dataField, (double[])buffer);
                return;
            }

            if (clrType == typeof(bool))
            {
                await rowGroupReader.ReadAsync<bool>(dataField, (bool[])buffer);
                return;
            }

            if (clrType == typeof(decimal))
            {
                await rowGroupReader.ReadAsync<decimal>(dataField, (decimal[])buffer);
                return;
            }

            if (clrType == typeof(DateTime))
            {
                await rowGroupReader.ReadAsync<DateTime>(dataField, (DateTime[])buffer);
                return;
            }

            throw new NotSupportedException($"Parquet CLR type '{clrType.FullName}' for column {clrType.Name} is not supported.");

        }
    }
}
