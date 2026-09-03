using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface IParquetSchemaReader
    {
        Task<ParquetTableDefinition> ReadSchemaAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
