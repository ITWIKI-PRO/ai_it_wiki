using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Text;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using MySqlConnector;
using ai_it_wiki.Data;
using ai_it_wiki.Services.TelegramBot;
using System.Net.Mail;
using System.Net;
using ai_it_wiki.Controllers;
using System.Xml.Linq;
using static ai_it_wiki.Extensions;
using ai_it_wiki;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations;
using ai_it_wiki.Models.Altron;
using OpenQA.Selenium;
using ErrorResponse = ai_it_wiki.Models.Altron.ErrorResponse;
using ai_it_wiki.Models.Altron.Files;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace it_wiki_site.Controllers
{
  [ApiController]
  [Route("altron/[action]")]
  public class AltronController : ControllerBase
  {
    private readonly ITelegramBotClient _botClient;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AltronController> _logger;
    private readonly IConfiguration _configuration;
    //  private readonly IHttpClientFactory _httpClientFactory;

    private readonly HttpClient _httpClient;
    private readonly SmtpClient _smtpClient;

    public AltronController([FromServices] ApplicationDbContext applicationDbContext,
        IWebHostEnvironment env,
        ILogger<AltronController> logger,
        IConfiguration configuration)
    {
      _env = env;
      _logger = logger;
      _configuration = configuration;


      _httpClient = new HttpClient();
      _smtpClient = new SmtpClient("mail.it-wiki.site", 25);
      _smtpClient.UseDefaultCredentials = false;
      _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
      _smtpClient.Credentials = new NetworkCredential("altron@it-wiki.site", "333Pycek9393");
      _smtpClient.EnableSsl = false;

      //создаем httpfactory
      //_httpClientFactory = new HttpClientFactory();
    }

    //метод индекс для проверки работоспособности
    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
      return Ok(new { IsSuccess = true, Message = "Altron API is running." });
    }


    /// <summary>
    /// Отправляет электронное письмо указанному получателю.
    /// </summary>
    /// <param name="mailToObject">Объект, содержащий адрес получателя, тему и тело письма.</param>
    /// <returns>Результат отправки письма.</returns>
    /// <response code="200">Письмо успешно отправлено.</response>
    /// <response code="400">Один или несколько обязательных параметров были пустыми.</response>
    /// <response code="500">Ошибка при отправке письма.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GenericSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerRequestExample(typeof(MailToObject), typeof(MailToObjectExample))]
    public async Task<IActionResult> MailTo([FromBody] MailToObject mailToObject)
    {
      // Это кастомный тип результата в вашем проекте
      ITWContentResult result = new() { ContentType = "application/json" };

      // Проверка обязательных полей
      if (string.IsNullOrEmpty(mailToObject.To) || string.IsNullOrEmpty(mailToObject.Body))
      {
        result.StatusCode = 400;
        result.Content = "Один или несколько обязательных параметров были пустыми, проверьте входные значения.";
        return result;
      }

      // Подготовка письма
      result.IsSuccess = false;
      MailAddress from = new("altron@it-wiki.pro", "Альтрон");
      MailAddress to = new(mailToObject.To);
      using MailMessage message = new(from, to)
      {
        Subject = mailToObject.Title,
        Body = mailToObject.Body
      };

      try
      {
        // Отправка письма через _smtpClient (SmtpClient)
        await _smtpClient.SendMailAsync(message);
        result.IsSuccess = true;
        result.StatusCode = 200;
        result.Content = "Mail sent successfully";
      }
      catch (Exception exc)
      {
        // Если произошла ошибка
        result.IsSuccess = false;
        result.StatusCode = 500;
        result.Content = exc.ToString();
      }

      return result;
    }

    /// <summary>
    /// Отправляет сообщение в Telegram пользователю с указанным идентификатором.
    /// </summary>
    /// <param name="request">Запрос, содержащий текст сообщения, идентификатор пользователя Telegram и режим форматирования текста.</param>
    /// <returns>Результат отправки сообщения в Telegram.</returns>
    /// <response code="200">Сообщение успешно отправлено.</response>
    /// <response code="400">Отсутствуют обязательные параметры: текст сообщения или идентификатор пользователя.</response>
    /// <response code="500">Ошибка при отправке сообщения в Telegram.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GenericSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerRequestExample(typeof(TelegramMessageRequest), typeof(TelegramMessageRequestExample))]
    public async Task<IActionResult> SendOpenAiResponseToTelegram([FromBody] TelegramMessageRequest request)
    {
      if (request.TelegramUserId == 0) request.TelegramUserId = 1406950293; // Замените на свой ID

      if (string.IsNullOrWhiteSpace(request.Message) || request.TelegramUserId == 0)
        return BadRequest(new { IsSuccess = false, StatusCode = 400, Message = "Message and TelegramUserId are required." });

      var parseMode = request.ParseMode?.ToLower() switch
      {
        "html" => ParseMode.Html,
        "markdownv2" => ParseMode.MarkdownV2,
        _ => ParseMode.Html
      };

      var text = parseMode == ParseMode.MarkdownV2 ? EscapeMarkdownV2(request.Message) : request.Message;
      var parts = SplitMessage(text, 4096);
      var results = new List<string>();

      foreach (var part in parts)
      {
        try
        {
          await _botClient.SendTextMessageAsync(request.TelegramUserId, CodeDetector.LooksLikeCode(part) ? $"```\n{part}\n```" : part, parseMode: CodeDetector.LooksLikeCode(part) ? ParseMode.MarkdownV2 : parseMode);
          results.Add("OK");
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Ошибка отправки сообщения в Telegram для пользователя {TelegramUserId}", request.TelegramUserId);
          results.Add($"FAIL: {ex.Message}");
        }
      }

      return Ok(new { IsSuccess = results.All(r => r == "OK"), StatusCode = 200, Results = results });

    }


    private static string EscapeMarkdownV2(string text)
    {
      var specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
      foreach (var ch in specialChars)
        text = text.Replace(ch, $"\\{ch}");
      return text;
    }

    private static List<string> SplitMessage(string message, int maxLength)
    {
      var result = new List<string>();
      while (message.Length > maxLength)
      {
        int splitAt = message.LastIndexOf('\n', maxLength);
        if (splitAt <= 0) splitAt = maxLength;
        result.Add(message[..splitAt].Trim());
        message = message[splitAt..].TrimStart();
      }
      if (message.Length > 0)
        result.Add(message);
      return result;
    }

    // ==== Файловые операции ====

    private string FilePath(string name)
    {
      if (name.Contains("wwwroot", StringComparison.OrdinalIgnoreCase))
        return name;

      var path = Path.Combine(_env.WebRootPath, "altron", name);
      return path;
    }

    [HttpGet]
    public IActionResult ListFiles()
    {
      var dir = Path.Combine(_env.WebRootPath, "altron");//путь будет выглядеть так
      if (!Directory.Exists(dir))
        return Ok(Array.Empty<object>());

      var files = Directory.GetFiles(dir)
          .Select(f => new FileInfo(f))
          .Select(fi => new
          {
            Name = fi.Name,
            Size = fi.Length,
            Created = fi.CreationTimeUtc,
            Modified = fi.LastWriteTimeUtc
          });

      return Ok(new GenericSuccessResponse(Newtonsoft.Json.JsonConvert.SerializeObject(files)));
    }

    [HttpGet]
    public IActionResult ReadFile([FromQuery] string name, [FromQuery] int? part = null, [FromQuery] int? partSize = 1048576)
    {
      if (string.IsNullOrWhiteSpace(name))
        return BadRequest(new { IsSuccess = false, Message = "Имя файла не задано." });

      var path = FilePath(name);
      if (!System.IO.File.Exists(path))
        return NotFound(new { IsSuccess = false, Message = "Файл не найден." });

      var content = System.IO.File.ReadAllText(path, Encoding.UTF8);
      if (part.HasValue)
      {
        int size = partSize ?? 1048576;
        int totalParts = (int)Math.Ceiling((double)content.Length / size);
        if (part.Value < 0 || part.Value >= totalParts)
          return BadRequest(new { IsSuccess = false, Message = "Индекс части вне диапазона.", TotalParts = totalParts });

        var slice = content.Substring(part.Value * size, Math.Min(size, content.Length - part.Value * size));
        return Ok(new { IsSuccess = true, Part = part.Value, TotalParts = totalParts, Content = slice });
      }

      return Ok(new { IsSuccess = true, Content = content });
    }

    [HttpPost]
    public IActionResult WriteFile([FromBody] FileWriteRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Name))
        return BadRequest(new { IsSuccess = false, Message = "Имя файла не задано." });

      var path = FilePath(req.Name);
      var content = req.Content ?? "";

      try
      {
        if (!string.IsNullOrEmpty(req.ReplaceText) && System.IO.File.Exists(path))
        {
          var text = System.IO.File.ReadAllText(path, Encoding.UTF8);
          text = text.Replace(req.ReplaceText, content);
          System.IO.File.WriteAllText(path, text, Encoding.UTF8);
        }
        else if (!string.IsNullOrEmpty(req.InsertAfter) && System.IO.File.Exists(path))
        {
          var lines = System.IO.File.ReadAllLines(path, Encoding.UTF8).ToList();
          var index = lines.FindIndex(l => l.Contains(req.InsertAfter));
          if (index != -1)
          {
            lines.Insert(index + 1, content);
            System.IO.File.WriteAllLines(path, lines, Encoding.UTF8);
          }
          else
          {
            return BadRequest(new { IsSuccess = false, NextRecommendedLLMActions = "ReadFile", Message = "InsertAfter не найден в файле." });
          }
        }
        else
        {
          // Перезапись или создание файла
          System.IO.File.WriteAllText(path, content);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка записи файла {FileName}", req.Name);
        return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
      }

      var url = $"{Request.Scheme}://{Request.Host}/altron/{Path.GetFileName(req.Name)}";

      return Ok(new { IsSuccess = true, Message = "Файл записан/перезаписан.", NextRecommendedLLMActions = "ReadFile", FileUrl = url });
    }

    ///<summary>
    /// Сохраняет изображение, переданное в виде Base64-строки, на сервере.
    /// </summary>
    /// <param name="req">Запрос, содержащий имя файла и содержимое в формате Base64.</param>
    /// <returns>Результат сохранения файла.</returns>
    /// <response code="200">Файл успешно сохранён.</response>
    /// <response code="400">Имя файла не задано.</response>
    /// <response code="500">Ошибка при сохранении файла.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GenericSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerRequestExample(typeof(FileWriteRequest), typeof(ImageWriteRequestExample))]
    public IActionResult WriteImage([FromBody] FileWriteRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Name))
        return BadRequest(new { IsSuccess = false, Message = "Имя файла не задано." });
      var path = FilePath(req.Name);
      var content = req.Content ?? "";
      try
      {
        if (!string.IsNullOrEmpty(content))
        {
          // Извлекаем содержимое Base64 из строки
          if (content.Contains("base64,"))
          {
            var fileType = content.Substring(content.IndexOf(":") + 1, content.IndexOf(";") - 5);

            //проверяем наличие расширения в имени файла
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
              if (fileType.Contains("png"))
                path += ".png";
              else if (fileType.Contains("jpg"))
                path += ".jpg";
              else
                return BadRequest(new { IsSuccess = false, Message = "Неверный тип файла. Ожидался png или jpg." });
            }
            else
            {
              //если расширение уже есть, то проверяем его соответствие
              if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !fileType.Contains("png"))
                return BadRequest(new { IsSuccess = false, Message = "Неверный тип файла. Ожидался png." });
              else if (path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !fileType.Contains("jpg"))
                return BadRequest(new { IsSuccess = false, Message = "Неверный тип файла. Ожидался jpg." });
            }
            content = content.Substring(content.IndexOf("base64,") + 7);
          }



          byte[] imageBytes = Convert.FromBase64String(content);
          System.IO.File.WriteAllBytes(path, imageBytes);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка записи файла {FileName}", req.Name);
        return StatusCode(500, new GenericSuccessResponse("Ошибка записи файла", false, "ListFiles"));
      }
      string message = "Файл сохранен";
      var response = new GenericSuccessResponse(message, true, "GetFilesUrl");
      return Ok(response);
    }

    /// <summary>
    /// Удаляет указанные файлы из директории <c>wwwroot/altron</c>.
    /// </summary>
    /// <param name="req">Запрос, содержащий список имён файлов для удаления.</param>
    /// <returns>Результат удаления каждого файла.</returns>
    /// <response code="200">Успешный ответ. Возвращает список результатов удаления.</response>
    /// <response code="400">Не переданы имена файлов для удаления.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public IActionResult DeleteFiles([FromQuery] DeleteFilesRequest req)
    {
      if (req == null || req.Names == null || req.Names.Length == 0)
        return BadRequest(new { IsSuccess = false, Message = "Не переданы имена файлов для удаления." });
      var results = req.Names.Select<string, object>(name =>
      {
        var path = FilePath(name);
        try
        {
          if (System.IO.File.Exists(path))
          {
            System.IO.File.Delete(path);
            return new { File = name, IsSuccess = true, Message = "Удалён", NextRecommendedLLMActions = "GetFilesUrl" };
          }
          return new { File = name, IsSuccess = false, Message = "Файл не найден" };
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Ошибка удаления файла {FileName}", name);
          return new { File = name, IsSuccess = false, NextRecommendedLLMActions = "GetFileInfo", Message = ex.Message };
        }
      }).ToList(); // Ensure the result is materialized as a list

      return Ok(new { IsSuccess = true, NextRecommendedLLMActions = "ListFiles", Results = results });
    }

    /// <summary>
    /// Переименовывает файл в директории <c>wwwroot/altron</c>.
    /// </summary>
    /// <param name="req">Запрос, содержащий исходное и новое имя файла.</param>
    /// <returns>Результат переименования файла.</returns>
    /// <response code="200">Файл успешно переименован.</response>
    /// <response code="400">Исходное и новое имя файла обязательны.</response>
    /// <response code="404">Исходный файл не найден.</response>
    /// <response code="500">Ошибка при переименовании файла.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult RenameFile([FromQuery] FileRenameRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.NewName))
        return BadRequest(new { IsSuccess = false, Message = "Исходное и новое имя файла обязательны." });

      var source = FilePath(req.Name);
      var dest = FilePath(req.NewName);
      if (!System.IO.File.Exists(source))
        return NotFound(new { IsSuccess = false, Message = "Исходный файл не найден." });

      try
      {
        System.IO.File.Move(source, dest, true);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка переименования файла {FileName}", req.Name);
        return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
      }
      return Ok(new { IsSuccess = true, Message = "Файл переименован.", NextRecommendedLLMActions = "GetFileInfo" });
    }

    /// <summary>
    /// Копирует файл в директории <c>wwwroot/altron</c>.
    /// </summary>
    /// <param name="req">Запрос, содержащий исходное и новое имя файла.</param>
    /// <returns>Результат копирования файла.</returns>
    /// <response code="200">Файл успешно скопирован.</response>
    /// <response code="400">Исходное и новое имя файла обязательны.</response>
    /// <response code="404">Исходный файл не найден.</response>
    /// <response code="500">Ошибка при копировании файла.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult CopyFile([FromQuery] FileRenameRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.NewName))
        return BadRequest(new { IsSuccess = false, Message = "Исходное и новое имя файла обязательны." });

      var source = FilePath(req.Name);
      var dest = FilePath(req.NewName);
      if (!System.IO.File.Exists(source))
        return NotFound(new { IsSuccess = false, Message = "Исходный файл не найден." });

      try
      {
        System.IO.File.Copy(source, dest, true);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка копирования файла {FileName}", req.Name);
        return StatusCode(500, new { IsSuccess = false, Message = ex.Message, NextRecommendedLLMActions = "ListFiles" });
      }
      return Ok(new { IsSuccess = true, NextRecommendedLLMActions = "ListFiles", Message = "Файл скопирован." });
    }

    #region GetFilesUrl

    /// <summary>
    /// Формирует публичные URL-адреса для уже загруженных или записанных файлов.
    /// </summary>
    /// <param name="req">Запрос с перечнем имён файлов.</param>
    /// <returns>Список пар «имя–URL». URL строится по шаблону <c>{Scheme}://{Host}/altron/{FileName}</c>.</returns>
    /// <response code="200">Успешный ответ. Возвращает <see cref="GetFilesUrlResponse"/>.</response>
    /// <response code="400">Не переданы имена файлов.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetFilesUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult GetFilesUrl([FromQuery][Required] GetFilesUrlRequest req)
    {
      if (req.Names == null || req.Names.Length == 0)
        return BadRequest(new ErrorResponse("Не переданы имена файлов."));

      var files = req.Names.Select(name => new FileUrlInfo(
          name,
          $"{Request.Scheme}://{Request.Host}/altron/{Uri.EscapeDataString(Path.GetFileName(name))}"
      ));

      return Ok(new GetFilesUrlResponse(files));
    }

    #endregion

    #region GetFileInfo

    /// <summary>
    /// Возвращает метаданные файла: имя, размер, время создания и изменения.
    /// </summary>
    /// <param name="name">Имя файла в папке <c>wwwroot/altron</c>.</param>
    /// <returns>Информация о файле.</returns>
    /// <response code="200">Успешный ответ. Возвращает <see cref="GetFileInfoResponse.FileInfoDto"/>.</response>
    /// <response code="400">Не указано имя файла.</response>
    /// <response code="404">Файл не найден.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetFileInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetFileInfo([FromQuery][Required] string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return BadRequest(new ErrorResponse("Имя файла не задано."));

      var path = Path.Combine(_env.WebRootPath, "altron", name);
      if (!System.IO.File.Exists(path))
        return NotFound(new ErrorResponse("Файл не найден."));

      var fi = new FileInfo(path);
      var info = new GetFileInfoResponse.FileInfoDto(
          fi.Name,
          fi.Length,
          fi.CreationTimeUtc,
          fi.LastWriteTimeUtc
      );

      return Ok(new GetFileInfoResponse(info));
    }
    #endregion

    #region ExecuteSqlQuery

    /// <summary>
    /// Приоритетный метод выполнения произвольных SQL-запросов через GET-запрос (ограничение — короткий запрос).
    /// </summary>
    /// <param name="query">
    /// Текст SQL-запроса, передаваемый в строке запроса.
    /// </param>
    /// <param name="connectionString">
    /// Строка подключения к БД. Необязательно — если не указана, используется <c>DefaultAltronConnection</c>.
    /// </param>
    /// <returns>
    /// Список строк, каждая — словарь «имя_столбца → значение».
    /// </returns>
    /// <response code="200">Успешный ответ. Тело: <see cref="SqlQueryResponse"/> с полем <c>Data</c>.</response>
    /// <response code="400">Ошибка: отсутствует или слишком длинный SQL-запрос.</response>
    /// <response code="500">Внутренняя ошибка сервера. Тело: <see cref="ErrorResponse"/>.</response>
    [HttpGet]
    [ProducesResponseType(typeof(SqlQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExecuteShortSqlQuery([FromQuery][Required] string query, [FromQuery] string connectionString = null)
    {
      const int MAX_QUERY_LENGTH = 2000;

      if (string.IsNullOrWhiteSpace(query))
        return BadRequest(new ErrorResponse("SQL-запрос обязателен."));

      if (query.Length > MAX_QUERY_LENGTH)
        return BadRequest(new ErrorResponse($"Слишком длинный SQL-запрос. Максимум — {MAX_QUERY_LENGTH} символов."));

      try
      {
        var connStr = string.IsNullOrEmpty(connectionString)
          ? _configuration.GetConnectionString("DefaultAltronConnection")
          : connectionString;

        using var connection = new MySqlConnection(connStr);
        await connection.OpenAsync();

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
          var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
          for (int i = 0; i < reader.FieldCount; i++)
            row[reader.GetName(i)] = reader.GetValue(i);
          rows.Add(row);
        }

        var rowsJson = System.Text.Json.JsonSerializer.Serialize(rows);
        return Ok(new SqlQueryResponse { Data = rowsJson });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка выполнения SQL через GET");
        return StatusCode(500, new GenericSuccessResponse(ex.Message, false, nextRecommendedLLMActions: "SHOW TABLES"));
      }
    }



    /// <summary>
    /// Выполняет произвольный SQL-запрос к базе данных и возвращает табличный результат.
    /// </summary>
    /// <param name="req">
    /// Запрос, содержащий:
    /// <list type="bullet">
    ///   <item><description><see cref="SqlQueryRequest.Query"/> — текст SQL-запроса.</description></item>
    ///   <item><description><see cref="SqlQueryRequest.ConnectionString"/> — строка подключения. Если не указана, берётся из настройки <c>DefaultAltronConnection</c>.</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// Список строк, где каждая строка представлена словарём «имя_столбца→значение».
    /// </returns>
    /// <response code="200">Успешный ответ. Тело: <see cref="SqlQueryResponse"/> с полем <c>Data</c>.</response>
    /// <response code="400">Неверный запрос: отсутствует или пуст <c>Query</c>.</response>
    /// <response code="500">Внутренняя ошибка сервера. Тело: <see cref="ErrorResponse"/> с текстом ошибки.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SqlQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(200, typeof(SqlQueryResponseExample))]
    [SwaggerRequestExample(typeof(SqlQueryRequest), typeof(SqlQueryRequestExample))]
    public async Task<IActionResult> ExecuteSqlQuery([FromBody][Required] SqlQueryRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Query))
        return BadRequest(new ErrorResponse("SQL-запрос обязателен."));

      try
      {
        if (string.IsNullOrEmpty(req.ConnectionString))
          req.ConnectionString = _configuration.GetConnectionString("DefaultAltronConnection");

        using var connection = new MySqlConnection(req.ConnectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(req.Query, connection);
        using var reader = await command.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
          var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
          for (int i = 0; i < reader.FieldCount; i++)
            row[reader.GetName(i)] = reader.GetValue(i);
          rows.Add(row);
        }
        var rowsJson = System.Text.Json.JsonSerializer.Serialize(rows);
        return Ok(new SqlQueryResponse { Data = rowsJson });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка выполнения SQL запроса");
        return StatusCode(500, new GenericSuccessResponse(ex.Message, false, nextRecommendedLLMActions: "SHOW TABLES"));
      }
    }

   

    #endregion

    #region ExecuteHttpRequest

    /// <summary>
    /// Выполняет HTTP-запрос к любому внешнему API и возвращает текст ответа (или часть его, разбитую на фрагменты).
    /// </summary>
    /// <param name="req">
    /// Параметры HTTP-запроса:
    /// <list type="bullet">
    ///   <item><description><see cref="HttpRequestExecutionRequest.Url"/> — адрес.</description></item>
    ///   <item><description><see cref="HttpRequestExecutionRequest.Method"/> — HTTP-метод (<c>GET</c>, <c>POST</c> и т.д.).</description></item>
    ///   <item><description><see cref="HttpRequestExecutionRequest.Headers"/> — дополнительные заголовки.</description></item>
    ///   <item><description><see cref="HttpRequestExecutionRequest.Body"/> — тело запроса (для <c>POST</c>/<c>PUT</c>).</description></item>
    ///   <item><description><see cref="HttpRequestExecutionRequest.Part"/> и <see cref="HttpRequestExecutionRequest.PartSize"/> — возвращать только часть ответа.</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// Объект <see cref="HttpRequestExecutionResponse"/>, содержащий:
    /// <list type="bullet">
    ///   <item><description>Код HTTP-статуса.</description></item>
    ///   <item><description>Полный ответ в <c>Content</c>, либо выбранная фрагментом часть.</description></item>
    /// </list>
    /// </returns>
    /// <response code="200">Успешный ответ от удалённого сервиса (<see cref="HttpRequestExecutionResponse"/>).</response>
    /// <response code="400">Неверный запрос: отсутствует <c>Url</c> или некорректен <c>Part</c>.</response>
    /// <response code="500">Внутренняя ошибка при выполнении HTTP-запроса (<see cref="ErrorResponse"/>).</response>
    [HttpPost]
    [ProducesResponseType(typeof(HttpRequestExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(200, typeof(HttpRequestExecutionResponseExample))]
    public async Task<IActionResult> ExecuteHttpRequest([FromBody][Required] HttpRequestExecutionRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Url))
        return BadRequest(new ErrorResponse("URL обязателен."));

      try
      {
        using var client = new HttpClient();
        var msg = new HttpRequestMessage(new HttpMethod(req.Method), req.Url);

        if (!string.IsNullOrWhiteSpace(req.Body))
          msg.Content = new StringContent(req.Body, Encoding.UTF8);

        if (req.Headers != null)
        {
          foreach (var kv in req.Headers)
          {
            if (!msg.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
              msg.Content?.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
          }
        }

        var resp = await client.SendAsync(msg);
        var content = await resp.Content.ReadAsStringAsync();
        var status = (int)resp.StatusCode;

        if (req.Part.HasValue)
        {
          int size = req.PartSize ?? (1024 * 1024);
          int totalParts = (int)Math.Ceiling((double)content.Length / size);
          if (req.Part < 0 || req.Part >= totalParts)
            return BadRequest(new ErrorResponse("Индекс части вне диапазона.", totalParts));

          string slice = content.Substring(req.Part.Value * size, Math.Min(size, content.Length - req.Part.Value * size));
          return Ok(new HttpRequestExecutionResponse(status, slice, req.Part.Value, totalParts));
        }

        return Ok(new HttpRequestExecutionResponse(status, content));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка выполнения HTTP запроса");
        return StatusCode(500, new ErrorResponse(ex.Message));
      }
    }

    #endregion
  }

  /// <summary>
  /// Объект для отправки почты.
  /// </summary>
  public class MailToObject
  {
    /// <summary>
    /// Адрес получателя. Обязательное поле.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Тема письма. Необязательное поле.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Тело письма. Обязательное поле.
    /// </summary>
    public string Body { get; set; }
  }


  /// <summary>
  /// Запрос для отправки сообщения в Telegram.
  /// </summary>
  public class TelegramMessageRequest
  {
    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Идентификатор пользователя Telegram.
    /// </summary>
    public long TelegramUserId { get; set; }

    /// <summary>
    /// Режим форматирования текста. Возможные значения: "html", "markdownv2" или null.
    /// </summary>
    public string ParseMode { get; set; }
  }


  public class TelegramMessageRequestExample : IExamplesProvider<TelegramMessageRequest>
  {
    public TelegramMessageRequest GetExamples()
    {
      return new TelegramMessageRequest
      {
        Message = "Hello, this is a test message!",
        TelegramUserId = 123456789,
        ParseMode = "html"
      };
    }

  }
  public class MailToObjectExample : IExamplesProvider<MailToObject>
  {
    public MailToObject GetExamples()
    {
      return new MailToObject()
      {
        Body = "Тестовое сообщение через опенапи",
        Title = "Тест апи Альтрона",
        To = "pycek@list.ru"
      };
    }
  }

}
