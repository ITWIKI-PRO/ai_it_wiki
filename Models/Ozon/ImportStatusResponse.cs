using System.Text.Json;

namespace ai_it_wiki.Models.Ozon
{
  public class ImportStatusResponse
  {
    public ImportStatusItem[]? Result { get; set; }
  }

  public class ImportStatusItem
  {
    public string? Status { get; set; }
    public object? Details { get; set; }
  }
}
