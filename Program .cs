using Bücherverwaltung;

DatabaseInitializer.Initialize();

var dbPath = Path.Combine(AppContext.BaseDirectory, "buecher.db");

BuecherService buecherService = new BuecherService(dbPath);
ConsoleComponents consoleComponents = new(buecherService);

while (true)
{
    consoleComponents.Home();
};



