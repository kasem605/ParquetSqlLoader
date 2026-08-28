using ParquetSQLLoader.Core.Models;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface ISqlTableGenerator
    {
        Task CreateTableAsync(string schema, string tableName, ParquetSchemaDefinition definition, CancellationToken cancellationToken = default);
    }
}
