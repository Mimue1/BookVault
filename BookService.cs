using Microsoft.Data.Sqlite;


namespace BookVault
{
    public class BookService
    {
        private readonly SqliteConnection _connection;

        private readonly CategoryMapper _categoryMapper;

        public BookService(SqliteConnection connection, CategoryMapper categoryMapper)
        {
            _connection = connection;
            _categoryMapper = categoryMapper;
        }

        public List<Book> GetBooks()
        {

            List<Book> books = new();

            using var command = _connection.CreateCommand();
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
                    Year = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Category = _categoryMapper.GetCategoryName(reader.GetInt32(5))
                };
                books.Add(book);
            }

            return books;
        }

        public void AddBook(Book book)
        {
            var categoryId = _categoryMapper.GetCategoryId(book.Category);
            
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO Books (Title, Author, Price, Year, CategoryId) VALUES (@title, @author, @price, @year, @categoryId)";

            command.Parameters.AddWithValue("@title", book.Name);
            command.Parameters.AddWithValue("@author", book.Author);
            command.Parameters.AddWithValue("@price", book.Price.HasValue ? book.Price : DBNull.Value);
            command.Parameters.AddWithValue("@year", book.Year.HasValue ? book.Year: DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", categoryId);

            command.ExecuteNonQuery();
        }

        public List<Book> GetCategoryBooks(string category)
        {
            List<Book> books = new();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT BookId, Title, Author, Price, Year, CategoryId FROM Books WHERE CategoryId = @category";

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
                    Year = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Category = _categoryMapper.GetCategoryName(reader.GetInt32(5))
                };
                books.Add(book);
            }

            return books;
        }

        public List<string> GetCategories()
        {
            List<string> categories = new();

            using var command = _connection.CreateCommand();
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

            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE Books SET Title = @title, Author = @author, Price = @price, Year = @year , CategoryId = @categoryId WHERE BookId = @id";

            command.Parameters.AddWithValue("@title", newBook.Name);
            command.Parameters.AddWithValue("@author", newBook.Author);
            command.Parameters.AddWithValue("@price", newBook.Price.HasValue ? newBook.Price : DBNull.Value);
            command.Parameters.AddWithValue("@year", newBook.Year.HasValue ? newBook.Year : DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", _categoryMapper.GetCategoryId(newBook.Category));
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
        }

        public void DeleteBook(int bookId)
        {
            using var command = _connection.CreateCommand();
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