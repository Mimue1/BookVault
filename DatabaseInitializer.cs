using Microsoft.Data.Sqlite;

public static class DatabaseInitializer
{
    public static void Initialize(SqliteConnection connection)
    {
        CreateTables(connection);
        SeedCategories(connection);
    }

    private static void CreateTables(SqliteConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Categories (
                CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Books (
                BookId INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                Price REAL,
                Year INTEGER,
                CategoryId INTEGER NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedCategories(SqliteConnection connection)
    {
        var categories = new (int Id, string Name)[]
        {
            (1, "Wishlist"),
            (2, "Queue"),
            (3, "Reading"),
            (4, "Read")
        };

        foreach (var (id, name) in categories)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Categories (CategoryId, Name)
                SELECT $id, $name
                WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryId = $id);
            ";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$name", name);
            cmd.ExecuteNonQuery();
        }
    }
}