using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductListRequest
  {
    [JsonPropertyName("filter")]
    public ProductInfoListFilter Filter { get; set; } = new ProductInfoListFilter();

    [JsonPropertyName("last_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string LastId { get; set; }

    [JsonPropertyName("limit"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? Limit { get; set; }
  }

  public class ProductInfoListFilter
  {
    [JsonPropertyName("offer_ids"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> OfferIds { get; set; }

    [JsonPropertyName("product_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<long> ProductId { get; set; }

    [JsonPropertyName("skus"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> Skus { get; set; }
  }
}
