using System.Text.Json;

namespace StreamApplicationLauncher.Models;

public static class Constants {
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}