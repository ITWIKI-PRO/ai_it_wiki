using ai_it_wiki.Internal;
using ai_it_wiki.Models;
using ai_it_wiki.Services.OpenAI;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ai_it_wiki.Controllers
{
  [Route("api/app")]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class AppApiController
  : ControllerBase
  {
    private readonly ILogger<AppApiController> _logger;

    public AppApiController(ILogger<AppApiController> logger)
    {
      _logger = logger;
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromServices] OpenAIService openAIService)
    {
      _logger.LogInformation("Запрос списка моделей через /api/app/get");
      try
      {
        var models = await openAIService.GetModelsAsync();
        _logger.LogInformation("Успешно получено моделей: {Count}", models.Count);
        return Ok(models);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при получении моделей");
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ошибка при получении моделей"));
      }
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModelsAsync([FromServices] OpenAIService openAIService)
    {
      _logger.LogInformation("Запрос списка моделей через /api/app/models");
      try
      {
        var models = await openAIService.GetModelsAsync();
        _logger.LogInformation("Успешно получено моделей: {Count}", models.Count);
        return Ok(models);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при получении моделей");
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ошибка при получении моделей"));
      }
    }

    [HttpPost("msg")]
    public async Task<IActionResult> SendMessage([FromServices] DialogService dialogService, [FromServices] OpenAIService openAIService, AppMessageObject appMessageObject)
    {
      _logger.LogInformation("Получен запрос на отправку сообщения");
      try
      {
        var dialog = dialogService.GetDialogById(appMessageObject.Message.DialogId);
        dialogService.GetDialogsForUser(appMessageObject.Message.UserId);
        var message = appMessageObject.Message;
        await openAIService.SendMessageAsync(message.Content);
        _logger.LogInformation("Сообщение успешно отправлено");
        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при отправке сообщения");
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ошибка при отправке сообщения"));
      }
    }

    [HttpPost("reg")]
    public IActionResult Registration([FromServices] AuthService authService, RegistrationObject registerObject)
    {
      _logger.LogInformation("Регистрация пользователя {Login}", registerObject.login);
      try
      {
        var user = authService.Register(registerObject.login, registerObject.password);
        return Ok(user);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка регистрации пользователя");
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ошибка регистрации пользователя"));
      }
    }

    [HttpPost("auth")]
    public IActionResult Authenticate([FromServices] AuthService authService, RegistrationObject registerObject)
    {
      _logger.LogInformation("Аутентификация пользователя {Login}", registerObject.login);
      try
      {
        var user = authService.Authenticate(registerObject.login, registerObject.password);
        if (user == null)
        {
          return Unauthorized(new ErrorResponse("Неверный логин или пароль"));
        }
        return Ok(user);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка аутентификации пользователя");
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Ошибка аутентификации пользователя"));
      }
    }
  }
}
