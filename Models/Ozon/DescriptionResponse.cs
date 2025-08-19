using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class DescriptionResponse
  {
    public DescriptionItem[]? Result { get; set; }
  }

  public class DescriptionItem
  {
    public string? Sku { get; set; }
    public string? Description { get; set; }
  }
}
