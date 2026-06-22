using BookVault;
using Microsoft.Data.Sqlite;

DatabaseInitializer.Initialize();

var dbPath = Path.Combine(AppContext.BaseDirectory, "books.db");
var connStr = $"Data Source={dbPath}";
using var connection = new SqliteConnection(connStr);
connection.Open();

var categoryMapper = new CategoryMapper(connection);
var bookService = new BookService(connection, categoryMapper);
var consoleComponents = new ConsoleComponents(bookService);

consoleComponents.App();



