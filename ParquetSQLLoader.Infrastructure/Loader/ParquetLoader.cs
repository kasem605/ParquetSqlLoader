using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Infrastructure.Loader
{
    public sealed class ParquetLoader : IParquetLoader
    {
        private readonly IParquetSchemaReader _schemaReader;
        private readonly IParquetRowReader _rowReader;
        private readonly ISqlBulkWriter _sqlBulkWriter;
        private readonly ISqlTableGenerator _tableGenerator;

        public ParquetLoader(IParquetSchemaReader schemaReader, IParquetRowReader rowReader, ISqlBulkWriter sqlBulkWriter, ISqlTableGenerator tableGenerator)
        {
            _schemaReader = schemaReader;
            _rowReader = rowReader;
            _sqlBulkWriter = sqlBulkWriter;
            _tableGenerator = tableGenerator;
        }

        public async Task LoadAsync(string parquetFilePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parquetFilePath, nameof(parquetFilePath));

            cancellationToken.ThrowIfCancellationRequested();

            // Read the schema from the Parquet file
            ParquetTableDefinition tableDefinition = await _schemaReader.ReadSchemaAsync(parquetFilePath, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Generate the SQL table creation script
            await _tableGenerator.CreateTableAsync(tableDefinition, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Read the rows from the Parquet file and write them to the SQL table
            await foreach (RowBatch rowBatch in _rowReader.ReadRowsAsync(parquetFilePath, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _sqlBulkWriter.WriteAsync(tableDefinition.TableName, rowBatch, cancellationToken);
            }
        }
    }
}
