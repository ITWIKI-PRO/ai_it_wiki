using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductImportResponse
  {
    [JsonPropertyName("result")]
    public ProductImportResult Result { get; set; }
  }

  public class ProductImportResult
  {
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; }

    [JsonPropertyName("items")]
    public List<ProductImportResultItem> Items { get; set; }
  }

  public class ProductImportResultItem
  {
    [JsonPropertyName("offer_id")]
    public string OfferId { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("sku")]
    public string Sku { get; set; }
  }
}
