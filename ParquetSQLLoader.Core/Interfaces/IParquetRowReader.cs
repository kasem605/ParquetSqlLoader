using ParquetSQLLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface IParquetRowReader
    {
        IAsyncEnumerable<RowBatch> ReadRowsAsync(string filePath, CancellationToken cancellationToken = default);

    }
}
