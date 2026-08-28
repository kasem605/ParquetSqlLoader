using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface IParquetSchemaReader
    {
        Task<ParquetSchemaDefinition> ReadSchemaAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
