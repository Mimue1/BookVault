using Microsoft.Data.Sqlite;


namespace BookVault
{
    public class BookService
    {
        private readonly string _sqliteDbPath;

        private readonly string _connStr;

        private readonly CategoryMapper _categoryMapper;

        public BookService(string dbPath)
        {
            _sqliteDbPath = dbPath;
            _connStr = $"Data Source={_sqliteDbPath}";
            _categoryMapper = new CategoryMapper(_sqliteDbPath);
        }

        public List<Book> GetBooks()
        {
            using var connection = new SqliteConnection(_connStr);

            List<Book> books = new();

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Buecher";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var book = new Book
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Autor = reader.GetString(2),
                    Preis = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Datum = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Kategorie = _categoryMapper.GetCategoryName(reader.GetInt32(5))
                };
                books.Add(book);
            }

            return books;
        }

        public void AddBook(Book book)
        {

            using var connection = new SqliteConnection(_connStr);

            var kategorieId = _categoryMapper.GeCategoryId(book.Kategorie);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Buecher (Titel, Autor, Preis, Erscheinungsjahr, KategorieId) VALUES (@titel, @autor, @preis, @erscheinungsjahr, @kategorieId)";

            command.Parameters.AddWithValue("@titel", book.Name);
            command.Parameters.AddWithValue("@autor", book.Autor);
            command.Parameters.AddWithValue("@preis", book.Preis.HasValue ? book.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", book.Datum.HasValue ? book.Datum: DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", kategorieId);

            command.ExecuteNonQuery();

            Console.WriteLine("Buch gespeichert");
        }

        public List<Book> GetCategoryBooks(string category)
        {
            using var connection = new SqliteConnection(_connStr);

            List<Book> books = new();

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BuecherId, Titel, Autor, Preis, Erscheinungsjahr, KategorieId FROM Buecher WHERE KategorieId = @kategorie";

            command.Parameters.AddWithValue("@kategorie", _categoryMapper.GeCategoryId(category));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var book = new Book
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Autor = reader.GetString(2),
                    Preis = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Datum = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Kategorie = _categoryMapper.GetCategoryName(reader.GetInt32(5))
                };
                books.Add(book);
            }

            return books;
        }

        public List<string> GetCategories()
        {
            using var connection = new SqliteConnection(_connStr);

            List<string> categories = new();

            connection.Open();


            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Kategorien";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString(0));
            }

            return categories;
        }

        public void EditBook(int bookId, Book newBook)
        {
            using var connection = new SqliteConnection(_connStr);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Buecher SET Titel = @titel, Autor = @autor, Preis = @preis, Erscheinungsjahr = @erscheinungsjahr, KategorieId = @kategorieId WHERE BuecherId = @id";

            command.Parameters.AddWithValue("@titel", newBook.Name);
            command.Parameters.AddWithValue("@autor", newBook.Autor);
            command.Parameters.AddWithValue("@preis", newBook.Preis.HasValue ? newBook.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", newBook.Datum.HasValue ? newBook.Datum : DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", _categoryMapper.GeCategoryId(newBook.Kategorie));
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
        }

        public void DeleteBook(int bookId)
        {
            using var connection = new SqliteConnection(_connStr);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Buecher WHERE BuecherId = @id";
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
        }

        public List<Book> SearchBooks(string query)
        {
            return GetBooks()
                .Where(b =>
                    b.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Autor.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}