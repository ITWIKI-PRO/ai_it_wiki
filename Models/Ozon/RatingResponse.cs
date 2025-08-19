using System.Text.Json;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class RatingResponse
  {
    [JsonPropertyName("result")]
    public RatingItem[]? Result { get; set; }
  }

  public class RatingItem
  {
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    // keep raw groups for future analysis
    [JsonPropertyName("groups")]
    public JsonElement? Groups { get; set; }
  }
}
