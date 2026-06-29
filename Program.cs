using BookVault;
using Microsoft.Data.Sqlite;

var folder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BookVault"
);

Directory.CreateDirectory(folder);

var dbPath = Path.Combine(folder, "bookvault.db");

var connectionString = $"Data Source={dbPath}";
using var connection = new SqliteConnection(connectionString);
connection.Open();

DatabaseInitializer.Initialize(connection);
var categoryMapper = new CategoryMapper(connection);
var bookService = new BookService(connection, categoryMapper);
var consoleComponents = new ConsoleComponents(bookService);

consoleComponents.App();



