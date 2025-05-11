namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Стандартная модель ошибки API.
  /// </summary>
  public class ErrorResponse
  {
    /// <summary>Признак успеха (false).</summary>
    public bool IsSuccess { get; } = false;

    /// <summary>Текст ошибки или описание причины отказа.</summary>
    public string Message { get; }

    /// <summary>Дополнительные данные (например, общее число частей при разбивке).</summary>
    public object Extra { get; }

    public ErrorResponse(string message, object extra = null)
    {
      Message = message;
      Extra = extra;
    }
  }
}
