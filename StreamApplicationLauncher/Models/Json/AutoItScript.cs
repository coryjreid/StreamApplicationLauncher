using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record AutoItScript(
    [property: JsonPropertyName("script")]
    string Script,
    [property: JsonPropertyName("isRunWait")]
    bool IsRunWait = true
);