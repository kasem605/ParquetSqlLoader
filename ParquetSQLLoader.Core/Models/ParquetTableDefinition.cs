using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace ParquetSQLLoader.Core.Models
{
    public class ParquetTableDefinition
    {
        public required string TableName { get; init; }
        public required IReadOnlyList<ParquetColumnDefinition> Columns { get; init; }
    }
}
