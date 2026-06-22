using Bücherverwaltung;

DatabaseInitializer.Initialize();

var dbPath = Path.Combine(AppContext.BaseDirectory, "buecher.db");

BookService buecherService = new BookService(dbPath);
ConsoleComponents consoleComponents = new(buecherService);

consoleComponents.App();



