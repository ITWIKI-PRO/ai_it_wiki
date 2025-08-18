namespace ai_it_wiki.Models
{
  /// <summary>
  /// Стандартная модель структурированного ответа об ошибке.
  /// </summary>
  public class ErrorResponse
  {
    /// <summary>Признак успеха (всегда false для ошибок).</summary>
    public bool IsSuccess { get; } = false;

    /// <summary>Текст ошибки.</summary>
    public string Message { get; }

    /// <summary>Дополнительная информация об ошибке.</summary>
    public string? Details { get; }

    /// <summary>SKU товара, если ошибка связана с конкретной карточкой.</summary>
    public string? Sku { get; }

    /// <summary>Дополнительные сведения (например, связанные данные).</summary>
    public object? Extra { get; }

    public ErrorResponse(string message, object? extra = null, string? details = null, string? sku = null)
    {
      Message = message;
      Extra = extra;
      Details = details;
      Sku = sku;
    }
  }
}
