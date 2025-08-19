using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ProductInfoResponse
  {
    public ProductInfoItem[]? Result { get; set; }
  }

  public class ProductInfoItem
  {
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public long? ProductId { get; set; }
    public object? Attributes { get; set; }
    public object? Images { get; set; }
    public object? Extra { get; set; }
  }
}
