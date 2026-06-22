using Spectre.Console;
using System.Threading.Tasks;


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

        public async Task Home()
        {
            while (true)
            {
                Console.Clear();

                ShowHeader();

                var readingBooks = await _buecherService.GetCategoryBooksAsync("Reading");
                                              
                var books = readingBooks.Select(b => b.Name);

                if (books.Any())
                {
                    var content = new Markup(
                        string.Join("\n", books.Select(b => $"- {b}"))
                    );

                    var panel = new Panel(content)
                    {
                        Header = new PanelHeader("Currently Reading")
                    }.Padding(1, 1).Expand();

                    AnsiConsole.Write(panel);
                }


                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("")
                        .AddChoices("Suche", "Bücher anzeigen", "Bücher verwalten", "Beenden"));


                switch (choice)
                {
                    case "Suche":
                        await SearchBook(); 
                        break;
                    case "Bücher anzeigen":
                        var buecher = await _buecherService.GetBuecherAsync();
                        ShowBooks(buecher.OrderBy(b => b.Kategorie).ToList());
                        break;
                    case "Bücher verwalten":
                        await ShowManagement();
                        break;
                    case "Beenden":
                        return;
                }
            }
        }

        public void ShowBooks(List<Buch> buecher)
        {
            Console.Clear();
            ShowHeader();

            var table = new Table().Expand();

            table.AddColumn("[bold]Titel[/]");
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

        public async Task ShowManagement()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Bitte wähle eine Aktion aus:")
                    .AddChoices("Buch Hinzufügen", "Buch Bearbeiten", "Buch Löschen", "Zurück"));


            switch (choice)
            {
                case "Buch Hinzufügen":
                    await AddBook();
                    break;
                case "Buch Bearbeiten":
                    await EditBook();
                    break;
                case "Buch Löschen":
                    var buch = await SearchBook();
                    if(buch != null) await DeleteBook(buch);
                    break;
                case "Zurück":
                    await Home();
                    break;
            }
        }

        public async Task AddBook()
        {
            var titel = AnsiConsole.Ask<string>("Titel");
            var autor = AnsiConsole.Ask<string>("Autor");
            var preis = AnsiConsole.Prompt(new TextPrompt<decimal?>("Preis [grey](optional)[/]").DefaultValue(null).AllowEmpty());
            var datum = AnsiConsole.Prompt(new TextPrompt<int?>("Erscheinungsjahr [grey](optional)[/]").DefaultValue(null).AllowEmpty());
            var kategorie = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Kategorie")
                    .AddChoices(await _buecherService.GetCategoriesAsync())
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
                await _buecherService.AddBookAsync(buch);
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

        public async Task<Buch?> SearchBook()
        {
            var suchbegriff = AnsiConsole.Prompt(new TextPrompt<string>("Suchbegriff(Enter = Alle):").AllowEmpty());
            var treffer = await _buecherService.SearchBooksAsync(suchbegriff);
            if (!treffer.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Keine Treffer gefunden.[/]");
                AnsiConsole.MarkupLine("[grey]Drücke eine Taste um zurück zu gehen...[/]");
                Console.ReadKey();
                return null;
            }

            var buch = AnsiConsole.Prompt(
                new SelectionPrompt<Buch>()
                    .Title("Buch auswählen")
                    .UseConverter(b => $"{b.Name} - {b.Autor}")
                    .AddChoices(treffer));

            var panel = new Panel(
            new Rows(
                new Markup($"[bold]Titel:[/] {buch.Name}"),
                new Markup($"[bold]Autor:[/] {buch.Autor}"),
                new Markup($"[bold]Preis:[/] {(buch.Preis.HasValue ? buch.Preis : "-")}"),
                new Markup($"[bold]Erscheinungsjahr:[/] {(buch.Datum.HasValue ? buch.Datum : "-")}"),
                new Markup($"[bold]Kategorie:[/] {buch.Kategorie}")))
            .Header($"[yellow]{buch.Name}:[/]");
            AnsiConsole.Write(panel);
            Console.ReadKey();
            return buch;
        }

        public async Task DeleteBook(Buch buch)
        {
            AnsiConsole.MarkupLine($"[red]{buch.Name} wird gelöscht:[/]");

            var confirmDelete = AnsiConsole.Confirm($"Möchten Sie dieses Buch wirklich löschen?", defaultValue: false);

            if(confirmDelete)
            {
                await _buecherService.DeleteBookAsync(buch.Id);
            }

            return;

        }

        public async Task EditBook()
        {
            var buch = await SearchBook();
            if (buch == null) return;

            var workingCopy = new Buch
            {
                Id = buch.Id,
                Name = buch.Name,
                Autor = buch.Autor,
                Preis = buch.Preis,
                Datum = buch.Datum,
                Kategorie = buch.Kategorie
            };

            while (true)
            {
                Console.Clear();
                ShowHeader();

                ShowEditPreview(workingCopy);

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Was möchtest du ändern?")
                        .AddChoices(
                            "Titel",
                            "Autor",
                            "Preis",
                            "Erscheinungsjahr",
                            "Kategorie",
                            "Speichern",
                            "Abbrechen"
                        ));

                switch (choice)
                {
                    case "Titel":
                        workingCopy.Name = AnsiConsole.Ask<string>(
                            "Neuer Titel:", workingCopy.Name);
                        break;

                    case "Autor":
                        workingCopy.Autor = AnsiConsole.Ask<string>(
                            "Neuer Autor:", workingCopy.Autor);
                        break;

                    case "Preis":
                        workingCopy.Preis = AnsiConsole.Prompt(
                            new TextPrompt<decimal?>("Neuer Preis:")
                                .DefaultValue(workingCopy.Preis)
                                .AllowEmpty());
                        break;

                    case "Erscheinungsjahr":
                        workingCopy.Datum = AnsiConsole.Prompt(
                            new TextPrompt<int?>("Neues Jahr:")
                                .DefaultValue(workingCopy.Datum)
                                .AllowEmpty());
                        break;

                    case "Kategorie":
                        workingCopy.Kategorie = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Neue Kategorie")
                                .AddChoices(await _buecherService.GetCategoriesAsync()));
                        break;

                    case "Speichern":
                        await _buecherService.EditBookAsync(buch.Id, workingCopy);
                        AnsiConsole.MarkupLine("[green]Änderungen gespeichert[/]");
                        Console.ReadKey();
                        return;

                    case "Abbrechen":
                        AnsiConsole.MarkupLine("[yellow]Abgebrochen[/]");
                        Console.ReadKey();
                        return;
                }
            }
        }

        private void ShowEditPreview(Buch b)
        {
            var panel = new Panel(
                new Rows(
                    new Markup($"[bold]Titel:[/] {b.Name}"),
                    new Markup($"[bold]Autor:[/] {b.Autor}"),
                    new Markup($"[bold]Preis:[/] {(b.Preis.HasValue ? b.Preis.ToString() : "-")}"),
                    new Markup($"[bold]Jahr:[/] {(b.Datum.HasValue ? b.Datum.ToString() : "-")}"),
                    new Markup($"[bold]Kategorie:[/] {b.Kategorie}")
                ))
                .Header("[yellow]Edit Preview[/]");
            AnsiConsole.Write(panel);
        }
    }
}
