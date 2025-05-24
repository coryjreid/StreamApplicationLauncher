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
                Name TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public void AddPid(int pid, string name) {
        ExecuteNonQuery(command => {
            command.CommandText = "INSERT INTO Pid (Pid, Name) VALUES ($pid, $name)";
            command.Parameters.AddWithValue("$pid", pid);
            command.Parameters.AddWithValue("$name", name);
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