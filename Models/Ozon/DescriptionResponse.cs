using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    public class DescriptionResponse
    {
        [JsonPropertyName("result")]
    public DescriptionItem? Result { get; set; }
    }

    public class DescriptionItem
    {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
