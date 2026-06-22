using Microsoft.Data.Sqlite;

public static class DatabaseInitializer
{
    private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "books.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        CreateTables(connection);
        SeedKategorien(connection);
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

    private static void SeedKategorien(SqliteConnection connection)
    {
        var kategorien = new (int Id, string Name)[]
        {
            (1, "Wishlist"),
            (2, "Queue"),
            (3, "Reading"),
            (4, "Read")
        };

        foreach (var (id, name) in kategorien)
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