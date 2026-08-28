namespace ParquetSQLLoader.Core.Models
{
    public class ImportOptions
    {
        public int BatchSize { get; init; } = 10_000;
        public bool CreateTAble { get; init; } = true;
        public bool DropExistingTable { get; init; }
        public bool UseTableLock { get; init; } = true;
        public int BulkCopyTimeout { get; init; } = 0;
    }
}
