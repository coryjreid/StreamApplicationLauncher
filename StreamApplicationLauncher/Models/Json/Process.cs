using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Process(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("existence")]
    ProcessExistence Existence,
    [property: JsonPropertyName("executable")]
    string Executable,
    [property: JsonPropertyName("workingDirectory")]
    string WorkingDirectory
);

public record ProcessExistence(
    [property: JsonPropertyName("waitExists")]
    bool WaitExists,
    [property: JsonPropertyName("waitTimeoutSeconds")]
    int WaitTimeoutSeconds);