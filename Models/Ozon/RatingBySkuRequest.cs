using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    public class RatingBySkuRequest
    {
        [JsonPropertyName("skus")]
        public IEnumerable<long> Skus { get; set; }
    }
}
