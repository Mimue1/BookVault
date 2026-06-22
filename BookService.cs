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
            command.CommandText = "SELECT * FROM Books";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var book = new Book
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Author = reader.GetString(2),
                    Price = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Date = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Category = _categoryMapper.GetCategoryName(reader.GetInt32(5))
                };
                books.Add(book);
            }

            return books;
        }

        public void AddBook(Book book)
        {

            using var connection = new SqliteConnection(_connStr);

            var kategorieId = _categoryMapper.GetCategoryId(book.Category);
            
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Books (Title, Author, Price, Date, CategoryId) VALUES (@title, @author, @price, @date, @categoryId)";

            command.Parameters.AddWithValue("@title", book.Name);
            command.Parameters.AddWithValue("@author", book.Author);
            command.Parameters.AddWithValue("@price", book.Price.HasValue ? book.Price : DBNull.Value);
            command.Parameters.AddWithValue("@date", book.Date.HasValue ? book.Date: DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", kategorieId);

            command.ExecuteNonQuery();
        }

        public List<Book> GetCategoryBooks(string category)
        {
            using var connection = new SqliteConnection(_connStr);

            List<Book> books = new();

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BookId, Title, Author, Price, Date, CategoryId FROM Books WHERE CategoryId = @category";

            command.Parameters.AddWithValue("@category", _categoryMapper.GetCategoryId(category));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var book = new Book
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Author = reader.GetString(2),
                    Price = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Date = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Category = _categoryMapper.GetCategoryName(reader.GetInt32(5))
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
            command.CommandText = "SELECT Name FROM Categories";

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
            command.CommandText = "UPDATE Books SET Title = @title, Author = @author, Price = @price, Date = @date , CategoryId = @categoryId WHERE BookId = @id";

            command.Parameters.AddWithValue("@title", newBook.Name);
            command.Parameters.AddWithValue("@author", newBook.Author);
            command.Parameters.AddWithValue("@price", newBook.Price.HasValue ? newBook.Price : DBNull.Value);
            command.Parameters.AddWithValue("@date", newBook.Date.HasValue ? newBook.Date : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", _categoryMapper.GetCategoryId(newBook.Category));
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
        }

        public void DeleteBook(int bookId)
        {
            using var connection = new SqliteConnection(_connStr);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Books WHERE BookId = @id";
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
        }

        public List<Book> SearchBooks(string query)
        {
            return GetBooks()
                .Where(b =>
                    b.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}