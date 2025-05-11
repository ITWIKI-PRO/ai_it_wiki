using System.ComponentModel.DataAnnotations;

namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Параметры для выполнения произвольного HTTP-запроса.
  /// </summary>
  public class HttpRequestExecutionRequest
  {
    /// <summary>
    /// URL для отправки запроса.
    /// </summary>
    [Required]
    [Url]
    public string Url { get; set; }

    /// <summary>
    /// HTTP-метод (<c>GET</c>, <c>POST</c>, <c>PUT</c>, <c>DELETE</c>, <c>PATCH</c>, <c>OPTIONS</c>, <c>HEAD</c> и т.д.).
    /// </summary>
    [Required]
    [RegularExpression("GET|POST|PUT|DELETE|PATCH|OPTIONS|HEAD",
        ErrorMessage = "Метод должен быть одним из: GET, POST, PUT, DELETE, PATCH, OPTIONS, HEAD.")]
    public string Method { get; set; }

    /// <summary>
    /// Тело запроса (для методов, поддерживающих тело: <c>POST</c>, <c>PUT</c> и т.д.).
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Дополнительные HTTP-заголовки (ключ → значение).
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Опционально: индекс части содержимого, если нужно вернуть не весь ответ сразу, а его фрагмент.
    /// Нумерация — с 0.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Part { get; set; }

    /// <summary>
    /// Опционально: размер части в байтах (по умолчанию 1 048 576 байт = 1 MiB).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? PartSize { get; set; } = 1048576;
  }
}
