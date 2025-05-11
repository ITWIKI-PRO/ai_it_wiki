using Swashbuckle.AspNetCore.Filters;

using System.ComponentModel.DataAnnotations;

namespace ai_it_wiki.Models.Altron.Files
{
  /// <summary>
  /// Запрос на запись в файл.
  /// </summary>
  public class FileWriteRequest
  {
    /// <summary>Имя файла (относительный путь внутри папки altron).</summary>
    [Required] public string Name { get; set; }

    /// <summary>Содержимое для записи.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Текст для замены перед записью.</summary>
    public string? ReplaceText { get; set; }

    /// <summary>Подстрока, после которой вставить новый текст.</summary>
    public string? InsertAfter { get; set; }

    /// <summary>Подстрока, перед которой вставить новый текст.</summary>
    public string? ContentType { get; set; } = "text/plain";
  }

  public class FileWriteRequestExample : IExamplesProvider<FileWriteRequest>
  {
    public FileWriteRequest GetExamples()
    {
      return new FileWriteRequest
      {
        Name = "example.txt",
        Content = "Hello, world!",
      };
    }
  }

  public class ImageWriteRequestExample : IExamplesProvider<FileWriteRequest>
  {
    public FileWriteRequest GetExamples()
    {
      return new FileWriteRequest
      {
        Name = "blue_square_50x50.png",
        Content = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAATUlEQVR4nO3OMQHAIBAAsQf/nlsDLJlguCjImvnmPft24KyWqCVqiVqilqglaolaopaoJWqJWqKWqCVqiVqilqglaolaopaoJWqJWuIHP6wBY/cJXlsAAAAASUVORK5CYII=",
      };
    }
  }

  /// <summary>
  /// Ответ на операцию WriteFile.
  /// </summary>
  public class FileWriteResponse
  {
    /// <summary>URL записанного файла.</summary>
    public string FileUrl { get; }

    /// <summary>Следующее рекомендуемое действие.</summary>
    public string NextRecommendedLLMActions { get; } = "ReadFile";

    public FileWriteResponse(string fileUrl)
    {
      FileUrl = fileUrl;
    }
  }

  /// <summary>
  /// DTO для информации о файле.
  /// </summary>
  public class FileInfoDto
  {
    /// <summary>Имя файла.</summary>
    public string Name { get; }

    /// <summary>Размер в байтах.</summary>
    public long Size { get; }

    /// <summary>Дата создания (UTC).</summary>
    public DateTime Created { get; }

    /// <summary>Дата последнего изменения (UTC).</summary>
    public DateTime Modified { get; }

    public FileInfoDto(string path)
    {
      var fi = new FileInfo(path);
      Name = fi.Name;
      Size = fi.Length;
      Created = fi.CreationTimeUtc;
      Modified = fi.LastWriteTimeUtc;
    }
  }

  /// <summary>
  /// Ответ на ListFiles.
  /// </summary>
  public class ListFilesResponse
  {
    /// <summary>Список информации по файлам.</summary>
    public IEnumerable<FileInfoDto> Files { get; }

    public ListFilesResponse(IEnumerable<FileInfoDto> files)
    {
      Files = files;
    }
  }

  /// <summary>
  /// Ответ на операцию ReadFile.
  /// </summary>
  public class ReadFileResponse
  {
    /// <summary>Содержимое файла или его части.</summary>
    public string Content { get; }

    /// <summary>Индекс части (если применимо).</summary>
    public int? Part { get; }

    /// <summary>Общее число частей (если применимо).</summary>
    public int? TotalParts { get; }

    public ReadFileResponse(string content, int? part = null, int? total = null)
    {
      Content = content;
      Part = part;
      TotalParts = total;
    }
  }

  /// <summary>
  /// Запрос на удаление нескольких файлов.
  /// </summary>
  public class DeleteFilesRequest
  {
    /// <summary>Имена файлов для удаления.</summary>
    [Required] public string[] Names { get; set; }
  }

  /// <summary>
  /// Результат удаления одного файла.</summary>
  public class DeleteResult
  {
    /// <summary>Имя файла.</summary>
    public string File { get; }

    /// <summary>Успех операции.</summary>
    public bool IsSuccess { get; }

    /// <summary>Сообщение результата.</summary>
    public string Message { get; }

    /// <summary>Следующее рекомендуемое действие.</summary>
    public string? NextRecommendedLLMActions { get; }

    public DeleteResult(string file, bool ok, string msg, string? next = null)
    {
      File = file;
      IsSuccess = ok;
      Message = msg;
      NextRecommendedLLMActions = next;
    }
  }

  /// <summary>
  /// Ответ на DeleteFiles.</summary>
  public class DeleteFilesResponse
  {
    /// <summary>Массив результатов удаления.</summary>
    public List<DeleteResult> Results { get; }

    public DeleteFilesResponse(List<DeleteResult> results)
    {
      Results = results;
    }
  }

  /// <summary>
  /// Запрос на переименование файла.</summary>
  public class FileRenameRequest
  {
    /// <summary>Текущее имя файла.</summary>
    [Required] public string Name { get; set; }

    /// <summary>Новое имя файла.</summary>
    [Required] public string NewName { get; set; }
  }

  /// <summary>
  /// Модель ошибки API.</summary>
  public class ErrorResponse
  {
    /// <summary>Признак успеха (false).</summary>
    public bool IsSuccess { get; } = false;

    /// <summary>Текст ошибки.</summary>
    public string Message { get; }

    /// <summary>Дополнительные данные.</summary>
    public object? Extra { get; }

    public ErrorResponse(string message, object? extra = null)
    {
      Message = message;
      Extra = extra;
    }
  }

  /// <summary>
  /// Запрос на получение URL-адресов файлов.
  /// </summary>
  public class GetFilesUrlRequest
  {
    /// <summary>
    /// Имена файлов, для которых нужно получить URL.
    /// </summary>
    [Required]
    public string[] Names { get; set; }
  }

  /// <summary>
  /// Информация об URL файла.
  /// </summary>
  public record FileUrlInfo(string File, string Url);

  /// <summary>
  /// Ответ с публичными URL-адресами файлов.
  /// </summary>
  public class GetFilesUrlResponse
  {
    /// <summary>Список файлов и их URL.</summary>
    public IEnumerable<FileUrlInfo> Files { get; }

    public bool IsSuccess { get; } = true;

    public GetFilesUrlResponse(IEnumerable<FileUrlInfo> files) => Files = files;
  }

  /// <summary>
  /// Ответ на запрос информации о файле.
  /// </summary>
  public class GetFileInfoResponse
  {
    /// <summary>Метаданные файла.</summary>
    public FileInfoDto File { get; }

    public bool IsSuccess { get; } = true;

    public GetFileInfoResponse(FileInfoDto info) => File = info;

    /// <summary>
    /// DTO с подробностями о файле.
    /// </summary>
    public record FileInfoDto(
        string Name,
        long Size,
        DateTime Created,
        DateTime Modified
    );
  }
}
