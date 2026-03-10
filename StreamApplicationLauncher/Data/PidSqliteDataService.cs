using System.IO;
using Microsoft.Data.Sqlite;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Data;

public class PidSqliteDataService(string? dbPath = null) {
    private readonly string _dbPath = ResolveDatabasePath(dbPath);

    public void Initialize() {
        using SqliteConnection connection = GetConnection();
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Pid (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Pid INTEGER NOT NULL,
                Name TEXT NOT NULL,
                ForceKill INTEGER NOT NULL DEFAULT 0
            );
            """;
        command.ExecuteNonQuery();

        // Migration: add ForceKill column to databases created before this column existed.
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Pid') WHERE name='ForceKill'";
        long columnExists = (long)(command.ExecuteScalar() ?? 0L);
        if (columnExists == 0) {
            command.CommandText = "ALTER TABLE Pid ADD COLUMN ForceKill INTEGER NOT NULL DEFAULT 0";
            command.ExecuteNonQuery();
        }
    }

    public void AddPid(int pid, string name, bool forceKill = false) {
        ExecuteNonQuery(command => {
            command.CommandText = "INSERT INTO Pid (Pid, Name, ForceKill) VALUES ($pid, $name, $forceKill)";
            command.Parameters.AddWithValue("$pid", pid);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$forceKill", forceKill ? 1 : 0);
        });
    }

    public List<(int Pid, string Name, bool ForceKill)> GetAllPids() {
        using SqliteConnection connection = GetConnection();
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Pid, Name, ForceKill FROM Pid";

        List<(int Pid, string Name, bool ForceKill)> results = [];
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            results.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2) != 0));
        }

        return results;
    }

    public void ClearAllPids() {
        ExecuteNonQuery(command => {
            command.CommandText = "DELETE FROM Pid";
        });
    }

    private SqliteConnection GetConnection() {
        return new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString());
    }

    private void ExecuteNonQuery(Action<SqliteCommand> func) {
        using SqliteConnection connection = GetConnection();
        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        func.Invoke(command);
        command.ExecuteNonQuery();
    }

    private static string ResolveDatabasePath(string? dbPath) {
        return !string.IsNullOrWhiteSpace(dbPath)
            ? dbPath
            : Path.Combine(Constants.ApplicationStoragePath, Constants.PidDatabaseFileName);
    }
}