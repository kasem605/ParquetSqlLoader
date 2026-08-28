using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Parquet
{
    public class ParquetSchemaReader : IParquetSchemaReader
    {
        public Task<ParquetSchemaDefinition> ReadSchemaAsync(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
