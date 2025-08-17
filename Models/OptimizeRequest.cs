using System.Collections.Generic;

namespace ai_it_wiki.Models
{
  /// <summary>
  /// Запрос на оптимизацию списка SKU
  /// </summary>
  public class OptimizeRequest
  {
    public List<string> Skus { get; set; } = new();
  }
}

