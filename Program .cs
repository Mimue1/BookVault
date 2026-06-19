using Bücherverwaltung;

BuecherService buecherService = new BuecherService("../../../Buecher.db");
ConsoleComponents consoleComponents = new(buecherService);

while (true)
{
    consoleComponents.Home();
};



