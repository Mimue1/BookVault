using Microsoft.Data.Sqlite;


namespace Bücherverwaltung
{
    public class BuecherService
    {
        private readonly string _sqliteDbPath;

        private readonly string _connStr;

        private readonly KategorieMapper _kategorieMapper;

        public BuecherService(string dbPath)
        {
            _sqliteDbPath = dbPath;
            _connStr = $"Data Source={_sqliteDbPath}";
            _kategorieMapper = new KategorieMapper(_sqliteDbPath);
        }

        public List<Buch> GetBuecher()
        {
            using var connection = new SqliteConnection(_connStr);

            List<Buch> buecher = new();

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Buecher";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var buch = new Buch
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Autor = reader.GetString(2),
                    Preis = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Datum = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Kategorie = _kategorieMapper.GetKategorieName(reader.GetInt32(5))
                };
                buecher.Add(buch);
            }

            return buecher;
        }

        public void AddBook(Buch buch)
        {

            using var connection = new SqliteConnection(_connStr);

            var kategorieId = _kategorieMapper.GetKategorieId(buch.Kategorie);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Buecher (Titel, Autor, Preis, Erscheinungsjahr, KategorieId) VALUES (@titel, @autor, @preis, @erscheinungsjahr, @kategorieId)";

            command.Parameters.AddWithValue("@titel", buch.Name);
            command.Parameters.AddWithValue("@autor", buch.Autor);
            command.Parameters.AddWithValue("@preis", buch.Preis.HasValue ? buch.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", buch.Datum.HasValue ? buch.Datum: DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", kategorieId);

            command.ExecuteNonQuery();

            Console.WriteLine("Buch gespeichert");
        }

        public List<Buch> GetCategoryBooks(string category)
        {
            using var connection = new SqliteConnection(_connStr);

            List<Buch> buecher = new();

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BuecherId, Titel, Autor, Preis, Erscheinungsjahr, KategorieId FROM Buecher WHERE KategorieId = @kategorie";

            command.Parameters.AddWithValue("@kategorie", _kategorieMapper.GetKategorieId(category));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var buch = new Buch
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Autor = reader.GetString(2),
                    Preis = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Datum = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Kategorie = _kategorieMapper.GetKategorieName(reader.GetInt32(5))
                };
                buecher.Add(buch);
            }

            return buecher;
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

        public void EditBook(int bookId, Buch neuesBuch)
        {
            using var connection = new SqliteConnection(_connStr);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Buecher SET Titel = @titel, Autor = @autor, Preis = @preis, Erscheinungsjahr = @erscheinungsjahr, KategorieId = @kategorieId WHERE BuecherId = @id";

            command.Parameters.AddWithValue("@titel", neuesBuch.Name);
            command.Parameters.AddWithValue("@autor", neuesBuch.Autor);
            command.Parameters.AddWithValue("@preis", neuesBuch.Preis.HasValue ? neuesBuch.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", neuesBuch.Datum.HasValue ? neuesBuch.Datum : DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", _kategorieMapper.GetKategorieId(neuesBuch.Kategorie));
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

        public List<Buch> SearchBooks(string suchbegriff)
        {
            return GetBuecher()
                .Where(b =>
                    b.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    b.Autor.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}