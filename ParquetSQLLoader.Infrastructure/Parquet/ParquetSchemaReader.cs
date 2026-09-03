using System.Threading;
using System.Threading.Tasks;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using Parquet;
using Parquet.Schema;

namespace ParquetSQLLoader.Infrastructure.Parquet
{
    public class ParquetSchemaReader : IParquetSchemaReader
    {
        public async Task<ParquetTableDefinition> ReadSchemaAsync(string filePath, CancellationToken cancellationToken = default)
        {
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

            await using ParquetReader parquetReader = await ParquetReader.CreateAsync(fileStream);

            cancellationToken.ThrowIfCancellationRequested();

            DataField[]? fields = parquetReader.Schema.GetDataFields();

            List<ParquetColumnDefinition> columns = fields.Select(field => new ParquetColumnDefinition
            {
                Name = field.Name,
                ClrType = field.ClrType?.Name ?? "Unknown",
                IsNullable = field.IsNullable,
            }).ToList();

            return new ParquetTableDefinition
            {
                TableName = Path.GetFileNameWithoutExtension(filePath),
                Columns = columns
            };
        }
    }
}
