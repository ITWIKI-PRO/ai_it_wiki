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

    /// <summary>Дополнительные сведения (необязательно).</summary>
    public object? Extra { get; }

    public ErrorResponse(string message, object? extra = null)
    {
      Message = message;
      Extra = extra;
    }
  }
}
