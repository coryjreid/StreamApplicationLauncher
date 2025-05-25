using System.Text.Json.Serialization;

namespace StreamApplicationLauncher.Models.Json;

public record Window(
    [property: JsonPropertyName("title")]
    string Title,
    [property: JsonPropertyName("existence")]
    WindowExistence Existence,
    [property: JsonPropertyName("onOpen")]
    WindowOnOpen OnOpen);
public record WindowExistence(
    [property: JsonPropertyName("waitExists")]
    bool WaitExists,
    [property: JsonPropertyName("waitTimeoutSeconds")]
    int WaitTimeoutSeconds);

public record WindowOnOpen(
    [property: JsonPropertyName("action")]
    Action Action,
    [property: JsonPropertyName("delaySeconds")]
    int ActionDelaySeconds);

public enum Action {
    Close,
    Hide,
    Minimize,
    None
}