using System.IO;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using StreamApplicationLauncher.Data;
using Xunit;

namespace StreamApplicationLauncher.Tests.Data;

public class PidSqliteDataServiceTests : IDisposable {
    private readonly string _dbPath;
    private readonly PidSqliteDataService _service;

    public PidSqliteDataServiceTests() {
        _dbPath = Path.GetTempFileName();
        File.Delete(_dbPath); // Remove placeholder so SQLite creates a fresh DB
        _service = new PidSqliteDataService(_dbPath);
        _service.Initialize();
    }

    public void Dispose() {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public void Initialize_CreatesTable() {
        // If Initialize() threw, the constructor would have failed.
        // Verify by calling GetAllPids — it would throw if the table didn't exist.
        Action act = () => _service.GetAllPids();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddPid_InsertsRecord() {
        _service.AddPid(1234, "notepad.exe");

        List<(int Pid, string Name, bool ForceKill)> pids = _service.GetAllPids();
        pids.Should().ContainSingle();
        pids[0].Pid.Should().Be(1234);
        pids[0].Name.Should().Be("notepad.exe");
        pids[0].ForceKill.Should().BeFalse();
    }

    [Fact]
    public void AddPid_WithForceKill_StoresFlag() {
        _service.AddPid(5678, "spotify.exe", forceKill: true);

        List<(int Pid, string Name, bool ForceKill)> pids = _service.GetAllPids();
        pids.Should().ContainSingle();
        pids[0].Pid.Should().Be(5678);
        pids[0].Name.Should().Be("spotify.exe");
        pids[0].ForceKill.Should().BeTrue();
    }

    [Fact]
    public void GetAllPids_EmptyTable_ReturnsEmptyList() {
        List<(int Pid, string Name, bool ForceKill)> pids = _service.GetAllPids();
        pids.Should().BeEmpty();
    }

    [Fact]
    public void GetAllPids_WithRecords_ReturnsAllPidsAndNames() {
        _service.AddPid(100, "app1.exe");
        _service.AddPid(200, "app2.exe");
        _service.AddPid(300, "app3.exe");

        List<(int Pid, string Name, bool ForceKill)> pids = _service.GetAllPids();

        pids.Should().HaveCount(3);
        pids.Should().Contain((100, "app1.exe", false));
        pids.Should().Contain((200, "app2.exe", false));
        pids.Should().Contain((300, "app3.exe", false));
    }

    [Fact]
    public void ClearAllPids_DeletesAllRecords() {
        _service.AddPid(111, "foo.exe");
        _service.AddPid(222, "bar.exe");

        _service.ClearAllPids();

        _service.GetAllPids().Should().BeEmpty();
    }

    [Fact]
    public void ClearAllPids_AfterAddPid_TableIsEmpty() {
        _service.AddPid(999, "test.exe");
        _service.ClearAllPids();

        List<(int Pid, string Name, bool ForceKill)> pids = _service.GetAllPids();
        pids.Should().BeEmpty();
    }
}
