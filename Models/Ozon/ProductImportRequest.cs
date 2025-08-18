using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductImportRequest
  {
    [JsonPropertyName("items")]
    public List<ProductImportItem> Items { get; set; }
  }

  public class ProductImportItem
  {
    [JsonPropertyName("offer_id")]
    public string OfferId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("attributes")]
    public List<ProductAttribute> Attributes { get; set; }

    [JsonPropertyName("complex_attributes")]
    public List<ComplexAttribute> ComplexAttributes { get; set; }

    [JsonPropertyName("images")]
    public List<ProductImage> Images { get; set; }
  }

  public class ProductAttribute
  {
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("values")]
    public List<string> Values { get; set; }
  }

  public class ComplexAttribute
  {
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("attributes")]
    public List<ProductAttribute> Attributes { get; set; }
  }

  public class ProductImage
  {
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
  }
}
