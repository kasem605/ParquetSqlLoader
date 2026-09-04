using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Infrastructure.Loader;
using ParquetSQLLoader.Infrastructure.SqlServer;
using ParquetSQLLoader.Infrastructure.Parquet;
using ParquetSQLLoader.Con.Services;

try
{
    Console.WriteLine("=============================================================================================");
    Console.WriteLine("ParquetSQLLoader - A tool to load Parquet files into SQL Server");
    const string version = "1.0.0";
    Console.WriteLine(version);
    Console.WriteLine("=============================================================================================");

    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json",
                     optional: false,
                     reloadOnChange: false)
        .Build();

    ServiceCollection? services = new ServiceCollection();

    services.AddSingleton<IConfiguration>(configuration);

    // Connection string for SQL Server
    string connectionString = configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("SQL Server connection string is not configured in appsettings.json");

    // Parquet
    services.AddSingleton<IParquetSchemaReader, ParquetSchemaReader>();
    services.AddSingleton<IParquetRowReader, ParquetRowReader>();

    // SQL Server
    services.AddSingleton<ISqlTableGenerator>(_ => new SQLTableGenerator(connectionString));
    services.AddSingleton<ISqlBulkWriter>(_ => new SQLBulkWriter(connectionString));

    // Loader
    services.AddSingleton<IParquetLoader, ParquetLoader>();

    using ServiceProvider serviceProvider = services.BuildServiceProvider();

    //IParquetLoader loader = serviceProvider.GetRequiredService<IParquetLoader>();

    string? parquetFilePath = args.Length > 0 ? args[0] : configuration["ParquetFilePath"];

    if (string.IsNullOrWhiteSpace(parquetFilePath))
    {
        Console.WriteLine("ERROR: Parquet file path is not configured");
        Console.WriteLine();
        Console.WriteLine("PLease provide the file path using either:;");
        Console.WriteLine();
        Console.WriteLine("     Command line:");
        Console.WriteLine("     ParquetSQLLOader.exe \"c:\\\\Parquet\\\\example.parquet\"");
        Console.WriteLine();
        Console.WriteLine("     Or in appsettings.json:");
        Console.WriteLine("     \"ParquetFilePath\": \"c:\\\\Parquet\\\\example.parquet\"");
        Console.WriteLine();
        Console.WriteLine("Application terminated");
        Console.WriteLine("=============================================================================================");

        return;
    }

    var importService = serviceProvider.GetRequiredService<ParquetSqlImportService>();

    await importService.ImportAsync(parquetFilePath);

    Console.WriteLine();
    Console.WriteLine("Import completed successfully.");
    //await loader.LoadAsync(parquetFilePath);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine();
    Console.WriteLine("ERROR: File not found");
    Console.WriteLine(ex.Message);
}
catch (ArgumentException ex)
{
    Console.WriteLine();
    Console.WriteLine("ERROR: Invalid argument");
    Console.WriteLine(ex.Message);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine();
    Console.WriteLine("ERROR: Configuration or application error");
    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("ERROR: An unexpected error occurred");
    Console.WriteLine(ex.Message);
}