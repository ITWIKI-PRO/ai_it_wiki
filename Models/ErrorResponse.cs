namespace ai_it_wiki.Models
{
  /// <summary>
  /// Стандартная модель структурированного ответа об ошибке.
  /// </summary>
  public class ErrorResponse
  {
    public bool IsSuccess { get; }
    public string Message { get; }
    public object? Extra { get; }

    // Adding a constructor to accept 'details' parameter  
    public ErrorResponse(string message, object? extra = null)
    {
      IsSuccess = false;
      Message = message;
      Extra = extra;
    }
  }
}
