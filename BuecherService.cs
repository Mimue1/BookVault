using Microsoft.Data.Sqlite;
using System.Threading.Tasks;


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

        public async Task<List<Buch>> GetBuecherAsync()
        {
            using var connection = new SqliteConnection(_connStr);

            List<Buch> buecher = new();

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Buecher";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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

        public async Task AddBookAsync(Buch buch)
        {

            using var connection = new SqliteConnection(_connStr);

            var kategorieId = _kategorieMapper.GetKategorieId(buch.Kategorie);

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Buecher (Titel, Autor, Preis, Erscheinungsjahr, KategorieId) VALUES (@titel, @autor, @preis, @erscheinungsjahr, @kategorieId)";

            command.Parameters.AddWithValue("@titel", buch.Name);
            command.Parameters.AddWithValue("@autor", buch.Autor);
            command.Parameters.AddWithValue("@preis", buch.Preis.HasValue ? buch.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", buch.Datum.HasValue ? buch.Datum: DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", kategorieId);

            await command.ExecuteNonQueryAsync();

            Console.WriteLine("Buch gespeichert");
        }

        public async Task<List<Buch>> GetCategoryBooksAsync(string category)
        {
            using var connection = new SqliteConnection(_connStr);

            List<Buch> buecher = new();

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BuecherId, Titel, Autor, Preis, Erscheinungsjahr, KategorieId FROM Buecher WHERE KategorieId = @kategorie";

            command.Parameters.AddWithValue("@kategorie", _kategorieMapper.GetKategorieId(category));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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

        public async Task<List<string>> GetCategoriesAsync()
        {
            using var connection = new SqliteConnection(_connStr);

            List<string> categories = new();

            await connection.OpenAsync();


            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Kategorien";

            using var reader = await command.ExecuteReaderAsync();
            while ( await reader.ReadAsync())
            {
                categories.Add(reader.GetString(0));
            }

            return categories;
        }

        public async Task EditBookAsync(int bookId, Buch neuesBuch)
        {
            using var connection = new SqliteConnection(_connStr);

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Buecher SET Titel = @titel, Autor = @autor, Preis = @preis, Erscheinungsjahr = @erscheinungsjahr, KategorieId = @kategorieId WHERE BuecherId = @id";

            command.Parameters.AddWithValue("@titel", neuesBuch.Name);
            command.Parameters.AddWithValue("@autor", neuesBuch.Autor);
            command.Parameters.AddWithValue("@preis", neuesBuch.Preis.HasValue ? neuesBuch.Preis : DBNull.Value);
            command.Parameters.AddWithValue("@erscheinungsjahr", neuesBuch.Datum.HasValue ? neuesBuch.Datum : DBNull.Value);
            command.Parameters.AddWithValue("@kategorieId", _kategorieMapper.GetKategorieId(neuesBuch.Kategorie));
            command.Parameters.AddWithValue("@id", bookId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteBookAsync(int bookId)
        {
            using var connection = new SqliteConnection(_connStr);

            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Buecher WHERE BuecherId = @id";
            command.Parameters.AddWithValue("@id", bookId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<Buch>> SearchBooksAsync(string suchbegriff)
        {
            if (string.IsNullOrWhiteSpace(suchbegriff))
            {
                return await GetBuecherAsync();
            }
            var buecher = await GetBuecherAsync();
            return buecher
                .Where(b =>
                    b.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    b.Autor.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}