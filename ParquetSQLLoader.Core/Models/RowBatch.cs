using System;
using System.Collections.Generic;
using System.Text;

namespace ParquetSQLLoader.Core.Models
{
    public sealed class RowBatch
    {
        public required IReadOnlyList<ParquetColumnDefinition> Columns { get; init; } 

        public required int RowCount { get; init; }

        public required IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; }
    }
}
