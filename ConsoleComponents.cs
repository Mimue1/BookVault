using Spectre.Console;

namespace BookVault
{
    public class ConsoleComponents
    {
        private readonly BookService _bookService;

        public ConsoleComponents(BookService bookService)
        {
            _bookService = bookService;
        }

        public  void ShowHeader()
        {
            AnsiConsole.Write(
                new FigletText("BookVault")
                    .Color(Color.DeepSkyBlue1)
            );

            AnsiConsole.Write(new Rule());
        }

        public void App()
        {
            while (true)
            {
                Console.Clear();

                ShowHeader();

                var readingBooks = _bookService.GetCategoryBooks("Reading")
                                              .Select(b => b.Name);

                if (readingBooks.Any())
                {
                    var content = new Markup(
                        string.Join("\n", readingBooks.Select(b => $"- {b}"))
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
                        .AddChoices("Search", "All Books", "Manage Books", "Exit"));


                switch (choice)
                {
                    case "Search":
                        SearchBook(); 
                        break;
                    case "All Books":
                        ShowBooks(_bookService.GetBooks().OrderBy(b => b.Category).ToList());
                        break;
                    case "Manage Books":
                        ShowManagement();
                        break;
                    case "Exit":
                        return;
                }
            }
        }

        public void ShowBooks(List<Book> books)
        {
            Console.Clear();
            ShowHeader();

            var table = new Table().Expand();

            table.AddColumn("[bold]Title[/]");
            table.AddColumn("[bold]Author[/]");
            table.AddColumn("[bold]Price[/]");
            table.AddColumn("[bold]Year[/]");
            table.AddColumn("[bold]Category[/]");

            foreach (var b in books)
            {
                table.AddRow(
                    b.Name ?? "-",
                    b.Author ?? "-",
                    b.Price.HasValue ? $"[green]{b.Price.Value:0.00}[/]" : "[grey]-[/]",
                    b.Year.HasValue ? $"[blue]{b.Year}[/]" : "[grey]-[/]",
                    b.Category ?? "-"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any Key to return...[/]");
            Console.ReadKey();
        }

        public void ShowManagement()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose an Option:")
                    .AddChoices("Add", "Edit", "Delete", "Back"));


            switch (choice)
            {
                case "Add":
                    AddBook();
                    break;
                case "Edit":
                    EditBook();
                    break;
                case "Delete":
                    var book = SearchBook();
                    if(book != null) DeleteBook(book);
                    break;
                case "Back":
                    App();
                    break;
            }
        }

        public void AddBook()
        {
            var title = AnsiConsole.Ask<string>("Title");
            var author = AnsiConsole.Ask<string>("Author");
            var price = AnsiConsole.Prompt(new TextPrompt<decimal?>("Price [grey](optional)[/]").DefaultValue(null).AllowEmpty());
            var year = AnsiConsole.Prompt(new TextPrompt<int?>("Year of Publication [grey](optional)[/]").DefaultValue(null).AllowEmpty());
            var category = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Category")
                    .AddChoices(_bookService.GetCategories())
            );

            Book book = new()
            {
                Name = title,
                Author = author,
                Price = price,
                Year = year,
                Category = category
            };

            AnsiConsole.WriteLine();
            var panel = new Panel(
                new Rows(
                    new Markup($"[bold]Title:[/] {book.Name}"),
                    new Markup($"[bold]Author:[/] {book.Author}"),
                    new Markup($"[bold]Price:[/] {(book.Price.HasValue ? book.Price : "-")}"),
                    new Markup($"[bold]Year of Publication:[/] {(book.Year.HasValue ? book.Year : "-")}"),
                    new Markup($"[bold]Category:[/] {book.Category}")))
            .Header("[yellow]Summary[/]")
            .Border(BoxBorder.Rounded);
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();


            if (AnsiConsole.Confirm("Do you want tho add this Book?"))
            {
                _bookService.AddBook(book);
                AnsiConsole.MarkupLine($"[green]{book.Name} was added![/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any Key to return...[/]");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Canceled.[/]");
            }
        }

        public Book? SearchBook()
        {
            var query = AnsiConsole.Prompt(new TextPrompt<string>("Search(Enter = All):").AllowEmpty());
            var result = query == string.Empty ? _bookService.GetBooks() : _bookService.SearchBooks(query);
            if (!result.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Not Found.[/]");
                AnsiConsole.MarkupLine("[grey]Press any Key to return...[/]");
                Console.ReadKey();
                return null;
            }

            var book = AnsiConsole.Prompt(
                new SelectionPrompt<Book>()
                    .Title("Choose Book")
                    .UseConverter(b => $"{b.Name} - {b.Author}")
                    .AddChoices(result));

            var panel = new Panel(
            new Rows(
                new Markup($"[bold]Title:[/] {book.Name}"),
                new Markup($"[bold]Author:[/] {book.Author}"),
                new Markup($"[bold]Price:[/] {(book.Price.HasValue ? book.Price : "-")}"),
                new Markup($"[bold]Year of Publication:[/] {(book.Year.HasValue ? book.Year : "-")}"),
                new Markup($"[bold]Category:[/] {book.Category}")))
            .Header($"[yellow]{book.Name}:[/]");
            AnsiConsole.Write(panel);
            Console.ReadKey();
            return book;
        }

        public void DeleteBook(Book book)
        {
            AnsiConsole.MarkupLine($"[red]Deleted {book.Name}...:[/]");

            var confirmDelete = AnsiConsole.Confirm($"Do you want to delete {book.Name}?", defaultValue: false);

            if(confirmDelete)
            {
                _bookService.DeleteBook(book.Id);
            }

            return;

        }

        public void EditBook()
        {
            var book = SearchBook();
            if (book == null) return;

            var workingCopy = new Book
            {
                Id = book.Id,
                Name = book.Name,
                Author = book.Author,
                Price = book.Price,
                Year = book.Year,
                Category = book.Category
            };

            while (true)
            {
                Console.Clear();
                ShowHeader();

                ShowEditPreview(workingCopy);

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose")
                        .AddChoices(
                            "Title",
                            "Author",
                            "Price",
                            "Year of Publication",
                            "Category",
                            "Save",
                            "Cancel"
                        ));

                switch (choice)
                {
                    case "Title":
                        workingCopy.Name = AnsiConsole.Ask<string>(
                            "New Title:", workingCopy.Name);
                        break;

                    case "Author":
                        workingCopy.Author = AnsiConsole.Ask<string>(
                            "New Author:", workingCopy.Author);
                        break;

                    case "Preis":
                        workingCopy.Price = AnsiConsole.Prompt(
                            new TextPrompt<decimal?>("New Price:")
                                .DefaultValue(workingCopy.Price)
                                .AllowEmpty());
                        break;

                    case "Year of Publication":
                        workingCopy.Year = AnsiConsole.Prompt(
                            new TextPrompt<int?>("New Year:")
                                .DefaultValue(workingCopy.Year)
                                .AllowEmpty());
                        break;

                    case "Category":
                        workingCopy.Category = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("New Category")
                                .AddChoices(_bookService.GetCategories()));
                        break;

                    case "Save":
                        _bookService.EditBook(book.Id, workingCopy);
                        AnsiConsole.MarkupLine("[green]Saved...[/]");
                        Console.ReadKey();
                        return;

                    case "Cancel":
                        AnsiConsole.MarkupLine("[yellow]Canceled[/]");
                        Console.ReadKey();
                        return;
                }
            }
        }

        private void ShowEditPreview(Book b)
        {
            var panel = new Panel(
                new Rows(
                    new Markup($"[bold]Title:[/] {b.Name}"),
                    new Markup($"[bold]Author:[/] {b.Author}"),
                    new Markup($"[bold]Price:[/] {(b.Price.HasValue ? b.Price.ToString() : "-")}"),
                    new Markup($"[bold]Year:[/] {(b.Year.HasValue ? b.Year.ToString() : "-")}"),
                    new Markup($"[bold]Category:[/] {b.Category}")
                ))
                .Header("[yellow]Edit Preview[/]");
            AnsiConsole.Write(panel);
        }
    }
}
