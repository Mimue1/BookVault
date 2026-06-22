using BookVault;

DatabaseInitializer.Initialize();

var dbPath = Path.Combine(AppContext.BaseDirectory, "books.db");

BookService bookService = new BookService(dbPath);
ConsoleComponents consoleComponents = new(bookService);

consoleComponents.App();



