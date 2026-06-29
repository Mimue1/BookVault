using BookVault;
using Microsoft.Data.Sqlite;


var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BookVault", "books.db");
var connStr = $"Data Source={dbPath}";
using var connection = new SqliteConnection(connStr);
connection.Open();

DatabaseInitializer.Initialize(connection);
var categoryMapper = new CategoryMapper(connection);
var bookService = new BookService(connection, categoryMapper);
var consoleComponents = new ConsoleComponents(bookService);

consoleComponents.App();



