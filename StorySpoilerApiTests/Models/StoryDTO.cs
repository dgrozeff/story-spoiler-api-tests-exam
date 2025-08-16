using System.Text.Json.Serialization;

namespace StorySpoilerApiTests.Models;

public class StoryDTO
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
