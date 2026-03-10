using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Cleanup {
    [property: JsonPropertyName("forceKill")]
    public bool ForceKill { get; init; } = false;
}
