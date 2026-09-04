using Parquet;
using Parquet.Schema;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                ClrType = GetApplicationClrType(field),
                IsNullable = field.IsNullable,

            }).ToList();

            foreach (ParquetColumnDefinition c in columns)
            {
                Debug.WriteLine(c.Name + ", " + c.ClrType + ", " + c.IsNullable);
            }

            return new ParquetTableDefinition
            {
                TableName = Path.GetFileNameWithoutExtension(filePath),
                Columns = columns
            };
        }

        private string GetApplicationClrType(DataField field)
        {
            if (field.ClrType == typeof(ReadOnlyMemory<char>))
            {
                return "String";
            }

            return field.ClrType.Name;
        }
    }
}
