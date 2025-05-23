using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Program(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("process")]
    Process Process,
    [property: JsonPropertyName("window")]
    Window Window
);