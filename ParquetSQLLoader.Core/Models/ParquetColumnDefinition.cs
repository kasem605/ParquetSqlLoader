namespace ParquetSQLLoader.Core.Models
{
    public sealed class ParquetColumnDefinition
    {
        public required string Name { get; init; }
        public required string ParquetType { get; init; }
        public required string ClrType { get; init; }
        public bool IsNullable { get; set; }
        public int? Precession { get; set; }
        public int? Scale { get; set; }
    }
}
