using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface ISqlTableGenerator
    {
        Task CreateTableAsync(ParquetTableDefinition schema, CancellationToken cancellationToken = default);
    }
}
