using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    public class RatingBySkuResponse
    {
        [JsonPropertyName("result")]
        public IEnumerable<RatingResult> Result { get; set; }
    }

    public class RatingResult
    {
        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("groups")]
        public IEnumerable<Group> Groups { get; set; }

        [JsonPropertyName("improve_attributes")]
        public IEnumerable<ImproveAttribute> ImproveAttributes { get; set; }

        [JsonPropertyName("improve_at_least")]
        public double? ImproveAtLeast { get; set; }
    }

    public class Group
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("block")]
        public string Block { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("conditions")]
        public IEnumerable<Condition> Conditions { get; set; }
    }

    public class Condition
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("fulfilled")]
        public bool? Fulfilled { get; set; }

        [JsonPropertyName("cost")]
        public double? Cost { get; set; }
    }

    public class ImproveAttribute
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
