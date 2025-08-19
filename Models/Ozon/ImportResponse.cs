using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
  public class ImportResponse
  {
    public ImportResult? Result { get; set; }
  }

  public class ImportResult
  {
    public string? TaskId { get; set; }
    public object? Extra { get; set; }
  }
}
