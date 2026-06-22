using Microsoft.Data.Sqlite;

namespace BookVault
{
    public class CategoryMapper
    {
        private readonly string _sqliteDbPath;

        public CategoryMapper(string sqliteDbPath)
        {
            _sqliteDbPath = sqliteDbPath;
        }

        public int GeCategoryId(string name)
        {
            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT KategorieId FROM Kategorien WHERE Name = @name";
            command.Parameters.AddWithValue("@name", name);

            var result = command.ExecuteScalar();

            if (result == null) throw new Exception($"Kategorie nicht gefunden: {name}");

            return Convert.ToInt32(result);
        }

        public string GetCategoryName(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_sqliteDbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Kategorien WHERE KategorieId = @id";
            command.Parameters.AddWithValue("@id", id);

            var result = command.ExecuteScalar();

            if (result == null) throw new Exception($"KategorieId nicht gefunden: {id}");

            return result.ToString()!;
        }
    }
}
