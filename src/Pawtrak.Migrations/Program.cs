using DbUp;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("Pawtrak")
    ?? throw new InvalidOperationException(
        "Connection string 'Pawtrak' is missing. " +
        "Set it in appsettings.json or via the CONNECTIONSTRINGS__PAWTRAK environment variable.");

Console.WriteLine("Pawtrak Migrations — starting...");

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly)
    .WithTransactionPerScript()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(result.Error);
    Console.ResetColor();
    return 1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Migrations applied successfully.");
Console.ResetColor();
return 0;
