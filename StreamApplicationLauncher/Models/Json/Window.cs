using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Window(
    [property: JsonPropertyName("closeOnOpen")]
    bool CloseOnOpen,
    [property: JsonPropertyName("hideOnOpen")]
    bool HideOnOpen,
    [property: JsonPropertyName("title")]
    string Title);