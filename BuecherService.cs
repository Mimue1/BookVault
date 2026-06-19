using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Xml.Linq;


namespace Bücherverwaltung
{
    public class BuecherService
    {
        private readonly string _sqliteDbPath;

        private readonly KategorieMapper _kategorieMapper;

        public BuecherService(string dbPath)
        {
            _sqliteDbPath = dbPath;
            _kategorieMapper = new KategorieMapper(_sqliteDbPath);
        }

        public List<Buch> GetBuecher()
        {
            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");

            List<Buch> buecher = new();


            try
            {
                Console.WriteLine("Connecting to Database...");
                connection.Open();
                Console.WriteLine("Connected");


                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Name, Autor, Preis, Erscheinungsdatum, KategorieId FROM Buecher";

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

            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");

            var kategorieId = _kategorieMapper.GetKategorieId(buch.Kategorie);

            try
            {
                Console.WriteLine("Connecting to Database...");
                connection.Open();
                Console.WriteLine("Connected");


                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Buecher (Name, Autor, Preis, Erscheinungsdatum, KategorieId) VALUES (@name, @autor, @preis, @erscheinungsdatum, @kategorieId)";

                command.Parameters.AddWithValue("@name", buch.Name);
                command.Parameters.AddWithValue("@autor", buch.Autor);
                command.Parameters.AddWithValue("@preis", buch.Preis.HasValue ? buch.Preis : DBNull.Value);
                command.Parameters.AddWithValue("@erscheinungsdatum", buch.Datum.HasValue ? buch.Datum: DBNull.Value);
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
            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");

            List<Buch> buecher = new();

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Name, Autor, Preis, Erscheinungsdatum, KategorieId FROM Buecher WHERE KategorieId = @kategorie";

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
            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");

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
    }
}