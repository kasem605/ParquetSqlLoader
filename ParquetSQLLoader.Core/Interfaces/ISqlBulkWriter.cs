using ParquetSQLLoader.Core.Models;
using System.Data;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface ISqlBulkWriter
    {
        Task WriteAsync(string tablename, RowBatch batch, CancellationToken cancellationToken = default);
    }
}
