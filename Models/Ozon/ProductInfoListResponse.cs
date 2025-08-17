using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductInfoListResponse
  {
    [JsonPropertyName("items")]
    public List<ProductItem> Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("last_id")]
    public string LastId { get; set; }
  }

  public class ProductItem
  {
    [JsonPropertyName("offer_id")]
    public string OfferId { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("sku")]
    public string Sku { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description_category_id")]
    public long DescriptionCategoryId { get; set; }

    [JsonPropertyName("attributes")]
    public List<ProductAttribute> Attributes { get; set; }
  }

  public class ProductAttribute
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("values")]
    public List<string> Values { get; set; }
  }
}
