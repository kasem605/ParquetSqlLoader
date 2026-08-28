using System.Data;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface ISqlBulkWriter
    {
        Task WriteAsync(string tablename, IDataReader reader, CancellationToken cancellationToken = default);
    }
}
