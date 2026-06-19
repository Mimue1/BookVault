using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Xml.Linq;


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


            try
            {
                Console.WriteLine("Connecting to Database...");
                connection.Open();
                Console.WriteLine("Connected");


                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Titel, Autor, Preis, Erscheinungsjahr, KategorieId FROM Buecher";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var buch = new Buch
                    {
                        Name = reader.GetString(0),
                        Autor = reader.GetString(1),
                        Preis = reader.IsDBNull(2) ? null : Convert.ToDouble(reader.GetDecimal(2)),
                        Datum = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        Kategorie = _kategorieMapper.GetKategorieName(reader.GetInt32(4))
                    };
                    buecher.Add(buch);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return buecher;
        }

        public void AddBook(Buch buch)
        {

            using var connection = new SqliteConnection(_connStr);

            var kategorieId = _kategorieMapper.GetKategorieId(buch.Kategorie);

            try
            {
                Console.WriteLine("Connecting to Database...");
                connection.Open();
                Console.WriteLine("Connected");


                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Buecher (Titel, Autor, Preis, Erscheinungsjahr, KategorieId) VALUES (@titel, @autor, @preis, @erscheinungsjahr, @kategorieId)";

                command.Parameters.AddWithValue("@titel", buch.Name);
                command.Parameters.AddWithValue("@autor", buch.Autor);
                command.Parameters.AddWithValue("@preis", buch.Preis.HasValue ? buch.Preis : DBNull.Value);
                command.Parameters.AddWithValue("@erscheinungsjahr", buch.Datum.HasValue ? buch.Datum: DBNull.Value);
                command.Parameters.AddWithValue("@kategorieId", kategorieId);

                command.ExecuteNonQuery();

                Console.WriteLine("Buch gespeichert");
                connection.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public List<Buch> GetCategoryBooks(string category)
        {
            using var connection = new SqliteConnection(_connStr);

            List<Buch> buecher = new();

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Titel, Autor, Preis, Erscheinungsjahr, KategorieId FROM Buecher WHERE KategorieId = @kategorie";

                command.Parameters.AddWithValue("@kategorie", _kategorieMapper.GetKategorieId(category));

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var buch = new Buch
                    {
                        Name = reader.GetString(0),
                        Autor = reader.GetString(1),
                        Preis = reader.IsDBNull(2) ? null : Convert.ToDouble(reader.GetDecimal(2)),
                        Datum = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        Kategorie = _kategorieMapper.GetKategorieName(reader.GetInt32(4))
                    };
                    buecher.Add(buch);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return buecher;
        }

        public List<string> GetCategories()
        {
            using var connection = new SqliteConnection(_connStr);

            List<string> categories = new();


            try
            {
                connection.Open();


                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Name FROM Kategorien";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return categories;
        }

        public void EditBook(int bookId, Buch neuesBuch)
        {

            try
            {
                using var connection = new SqliteConnection(_connStr);

                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE Buecher SET Titel = @titel, Autor = @autor, Preis = @preis, Erscheinungsjahr = @jahr, KategorieId = @kategorie WHERE BuecherId = @id";

                command.Parameters.AddWithValue("@titel", neuesBuch.Name);
                command.Parameters.AddWithValue("@autor", neuesBuch.Autor);
                command.Parameters.AddWithValue("@preis", neuesBuch.Preis.HasValue ? neuesBuch.Preis : DBNull.Value);
                command.Parameters.AddWithValue("@erscheinungsjahr", neuesBuch.Datum.HasValue ? neuesBuch.Datum : DBNull.Value);
                command.Parameters.AddWithValue("@kategorieId", _kategorieMapper.GetKategorieId(neuesBuch.Kategorie));
                command.Parameters.AddWithValue("@id", bookId);

                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DeleteBook(int bookId)
        {
            using var connection = new SqliteConnection(_connStr);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Buecher WHERE BuecherId = @id";
            command.Parameters.AddWithValue("@id", bookId);

            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}