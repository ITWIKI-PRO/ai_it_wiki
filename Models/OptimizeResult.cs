using System;

namespace ai_it_wiki.Models
{
  /// <summary>
  /// Результат оптимизации для одного SKU.
  /// </summary>
  public class OptimizeResult
  {
    /// <summary>SKU товара.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Итоговый рейтинг контента после оптимизации.</summary>
    public int Rating { get; set; }

    /// <summary>Если произошла ошибка при обработке этого SKU, вернётся объект ошибки.</summary>
    public ErrorResponse? Error { get; set; }
  }
}
