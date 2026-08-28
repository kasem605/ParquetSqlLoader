using ParquetSQLLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Core.Interfaces
{
    public interface IParquetReader
    {
        Task<ParquetTableDefinition> ReadTableDefinitionAsync(string filePath);
    }
}
