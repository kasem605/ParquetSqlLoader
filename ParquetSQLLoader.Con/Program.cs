using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParquetSQLLoader.Core.Interfaces;
using ParquetSQLLoader.Infrastructure.Loader;
using ParquetSQLLoader.Infrastructure.Parquet;
using ParquetSQLLoader.Infrastructure.Parquet.SqlServer;

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

IParquetLoader loader = serviceProvider.GetRequiredService<IParquetLoader>();

string parquetFilePath = args.Length > 0 ? args[0] : throw new ArgumentException("Parquet file path is not configured in appsettings.json");

await loader.LoadAsync(parquetFilePath);