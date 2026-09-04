using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface IParquetLoader
    {
        Task LoadAsync(string parquetFilePath, CancellationToken cancellationToken = default);
    }
}
