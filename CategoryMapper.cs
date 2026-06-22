using Microsoft.Data.Sqlite;

namespace BookVault
{
    public class CategoryMapper
    {
        private readonly SqliteConnection _connection;

        public CategoryMapper(SqliteConnection connection)
        {
            _connection = connection;
        }

        public int GetCategoryId(string name)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT CategoryId FROM Categories WHERE Name = @name";
            command.Parameters.AddWithValue("@name", name);

            var result = command.ExecuteScalar();

            if (result == null) throw new Exception($"Category Not Found: {name}");

            return Convert.ToInt32(result);
        }

        public string GetCategoryName(int id)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Categories WHERE CategoryId = @id";
            command.Parameters.AddWithValue("@id", id);

            var result = command.ExecuteScalar();

            if (result == null) throw new Exception($"CategoryId Not Found: {id}");

            return result.ToString()!;
        }
    }
}
