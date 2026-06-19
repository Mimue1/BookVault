using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bücherverwaltung
{
    public class ConsoleComponents
    {
        private readonly BuecherService _buecherService;

        public ConsoleComponents(BuecherService buecherService)
        {
            _buecherService = buecherService;
        }

        public  void ShowHeader()
        {
            AnsiConsole.Write(
                new FigletText("BookVault")
                    .Color(Color.DeepSkyBlue1)
            );

            AnsiConsole.MarkupLine("[grey]Bücherverwaltung System v1.0[/]\n");
            AnsiConsole.Write(new Rule());
        }

        public void Home()
        {
            Console.Clear();

            ShowHeader();

            var readingBooks = _buecherService.GetCategoryBooks("Reading")
                                          .Select(b => b.Name);

            if (readingBooks.Any())
            {
                var content = new Markup(
                    string.Join("\n", readingBooks.Select(b => $"- {b}"))
                );

                var panel = new Panel(content)
                {
                    Header = new PanelHeader("Currently Reading")
                }.Padding(1, 1);

                AnsiConsole.Write(panel);
            }


            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("")
                    .AddChoices("Bücher verwalten", "Bücher anzeigen", "Beenden"));


            switch (choice)
            {
                case "Bücher verwalten":
                    ShowManagement();
                    break;
                case "Bücher anzeigen":
                    ShowBooks(_buecherService.GetBuecher().OrderBy(b => b.Kategorie).ToList());
                    break;
                case "Beenden":
                    return;
            }
        }

        public void ShowBooks(List<Buch> buecher)
        {
            Console.Clear();
            ShowHeader();

            var table = new Table().Expand();

            table.AddColumn("[bold]Name[/]");
            table.AddColumn("[bold]Autor[/]");
            table.AddColumn("[bold]Preis[/]");
            table.AddColumn("[bold]Erscheinungsjahr[/]");
            table.AddColumn("[bold]Kategorie[/]");

            foreach (var b in buecher)
            {
                table.AddRow(
                    b.Name ?? "-",
                    b.Autor ?? "-",
                    b.Preis.HasValue ? $"[green]{b.Preis.Value:0.00}[/]" : "[grey]-[/]",
                    b.Datum.HasValue ? $"[blue]{b.Datum}[/]" : "[grey]-[/]",
                    b.Kategorie ?? "-"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Drücke eine Taste um zurück zu gehen...[/]");
            Console.ReadKey();
        }

        public void ShowManagement()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Bitte wähle eine Aktion aus:")
                    .AddChoices("Buch Hinzufügen", "Buch Bearbeiten", "Buch Löschen", "Buch Suchen", "Zurück"));


            switch (choice)
            {
                case "Buch Hinzufügen":
                    AddBook();
                    break;
                case "Buch Bearbeiten":
                    break;
                case "Buch Löschen":
                    break;
                case "Buch Suchen":
                    break;
                case "Zurück":
                    Home();
                    break;
            }
        }

        public void AddBook()
        {
            var titel = AnsiConsole.Ask<string>("Titel");
            var autor = AnsiConsole.Ask<string>("Autor");
            var preisInp = AnsiConsole.Prompt(new TextPrompt<string>("Preis [grey](optional)[/]").AllowEmpty());
            preisInp = preisInp
                .Replace("€", "")
                .Replace("$", "")
                .Replace(" ", "")
                .Replace(",", ".")
                .Trim();
            double? preis = string.IsNullOrWhiteSpace(preisInp) ? null : double.Parse(preisInp);
            var datumInp = AnsiConsole.Prompt(new TextPrompt<string>("Erscheinungsjahr [grey](optional)[/]").AllowEmpty());
            int? datum = string.IsNullOrWhiteSpace(datumInp) ? null : int.Parse(datumInp);
            var kategorie = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Kategorie")
                    .AddChoices(_buecherService.GetCategories())
            );

            Buch buch = new()
            {
                Name = titel,
                Autor = autor,
                Preis = preis,
                Datum = datum,
                Kategorie = kategorie
            };

            AnsiConsole.WriteLine();
            var panel = new Panel(
                    new Rows(
                        new Markup($"[bold]Titel:[/] {buch.Name}"),
                        new Markup($"[bold]Autor:[/] {buch.Autor}"),
                        new Markup($"[bold]Preis:[/] {(buch.Preis.HasValue ? buch.Preis : "-")}"),
                        new Markup($"[bold]Erscheinungsjahr:[/] {(buch.Datum.HasValue ? buch.Datum : "-")}"),
                        new Markup($"[bold]Kategorie:[/] {buch.Kategorie}")))
                .Header("[yellow]Summary[/]")
                .Border(BoxBorder.Rounded);
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            // Confirm order
            if (AnsiConsole.Confirm("Buch Hinzufügen?"))
            {
                _buecherService.AddBook(buch);
                AnsiConsole.MarkupLine($"[green]Das Buch {buch.Name} wurde Hinzugefügt![/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Drücke eine Taste um zurück zu gehen...[/]");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Abgebrochen.[/]");
            }
        }
    }
}
