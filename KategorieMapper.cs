using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bücherverwaltung
{
    public class KategorieMapper
    {
        private readonly string _sqliteDbPath;

        public KategorieMapper(string sqliteDbPath)
        {
            _sqliteDbPath = sqliteDbPath;
        }

        public int GetKategorieId(string name)
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

        public string GetKategorieName(int id)
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
