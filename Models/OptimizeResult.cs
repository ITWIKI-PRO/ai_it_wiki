using System;

namespace ai_it_wiki.Models
{
  /// <summary>
  /// Результат оптимизации карточки товара.
  /// </summary>
  public class OptimizeResult
  {
    /// <summary>SKU товара.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Итоговый рейтинг контента.</summary>
    public int? Rating { get; set; }

    /// <summary>Информация об ошибке, если оптимизация не удалась.</summary>
    public ErrorResponse? Error { get; set; }
  }
}
