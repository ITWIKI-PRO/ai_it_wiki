using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductInfoListRequest
  {
    [JsonPropertyName("filter")]
    public ProductInfoListFilter Filter { get; set; }

    [JsonPropertyName("last_id")]
    public string LastId { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
  }

  public class ProductInfoListFilter
  {
    [JsonPropertyName("offer_ids")]
    public List<string> OfferIds { get; set; }

    [JsonPropertyName("product_ids")]
    public List<long> ProductIds { get; set; }

    [JsonPropertyName("skus")]
    public List<string> Skus { get; set; }
  }
}
