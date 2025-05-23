using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Process(
    [property: JsonPropertyName("executablePath")]
    string ExecutablePath,
    [property: JsonPropertyName("workingDirectory")]
    string WorkingDirectory
);