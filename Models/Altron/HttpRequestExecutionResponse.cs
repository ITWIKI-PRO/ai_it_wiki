using Swashbuckle.AspNetCore.Filters;

namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Универсальный ответ на HTTP-запрос.
  /// </summary>
  public class HttpRequestExecutionResponse
  {
    /// <summary>Код HTTP-статуса.</summary>
    public int StatusCode { get; }

    /// <summary>Полный или усечённый контент ответа.</summary>
    public string Content { get; }

    /// <summary>Индекс возвращённой части (если применимо).</summary>
    public int? Part { get; }

    /// <summary>Общее число частей (если применимо).</summary>
    public int? TotalParts { get; }

    public HttpRequestExecutionResponse(int statusCode, string content, int? part = null, int? total = null)
    {
      StatusCode = statusCode;
      Content = content;
      Part = part;
      TotalParts = total;
    }
  }

  public class HttpRequestExecutionResponseExample : IExamplesProvider<HttpRequestExecutionResponse>
  {
    public HttpRequestExecutionResponse GetExamples()
    {
      return new HttpRequestExecutionResponse(200, "OK", 0, 1);
    }
  }
}
