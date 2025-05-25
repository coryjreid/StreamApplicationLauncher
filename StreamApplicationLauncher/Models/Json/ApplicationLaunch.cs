using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record ApplicationLaunch(
    [property: JsonPropertyName("name")]
    string LaunchName,
    [property: JsonPropertyName("process")]
    Process Process,
    [property: JsonPropertyName("window")]
    Window Window
);