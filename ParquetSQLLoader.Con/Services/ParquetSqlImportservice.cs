using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Con.Services
{
    public sealed class ParquetSqlImportService : IParquetSqlImportService
    {
        private readonly IParquetSchemaReader _parquetSchemaReader;
        private readonly IParquetRowReader _parquetRowReader;
        private readonly ISqlTableGenerator _sqlTableCreator;
        private readonly ISqlBulkWriter _sqlBulkInserter;

        public ParquetSqlImportService(IParquetSchemaReader parquetSchemaReader, IParquetRowReader parquetRowReader, ISqlTableGenerator sqlTableCreator, ISqlBulkWriter sqlBulkInserter)
        {
            _parquetSchemaReader = parquetSchemaReader;
            _parquetRowReader = parquetRowReader;
            _sqlTableCreator = sqlTableCreator;
            _sqlBulkInserter = sqlBulkInserter;
        }

        private async Task ImportAsync(string parquetFilePath, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(parquetFilePath))
            {
                throw new ArgumentException("Parquet file path cannot be null or empty.", nameof(parquetFilePath));
            }

            if(!File.Exists(parquetFilePath))
            {
                throw new FileNotFoundException($"The specified Parquet file '{parquetFilePath}' does not exist.", parquetFilePath);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine($"Input File: {parquetFilePath}");
            Console.WriteLine();

            ParquetTableDefinition tableDefinition = await _parquetSchemaReader.ReadSchemaAsync(parquetFilePath, cancellationToken);

            Console.WriteLine($"Table Name: {tableDefinition.TableName}");

            await _sqlTableCreator.CreateTableAsync(tableDefinition, cancellationToken);

            await foreach(RowBatch batch in _parquetRowReader.ReadRowsAsync(parquetFilePath, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _sqlBulkInserter.WriteAsync(tableDefinition.TableName, batch, cancellationToken);

                Console.WriteLine($"Loaded {batch.RowCount} rows into table '{tableDefinition.TableName}'.");   
            }


        }
    }
}
