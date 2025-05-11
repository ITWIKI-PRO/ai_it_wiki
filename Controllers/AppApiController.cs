using ai_it_wiki.Internal;
using ai_it_wiki.Models;
using ai_it_wiki.Services.OpenAI;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ai_it_wiki.Controllers
{
  [Route("api/app")]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class AppApiController
  : ControllerBase
  {
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromServices] OpenAIService openAIService)
    {
      var models = await openAIService.GetModelsAsync();
      return Ok(models);
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModelsAsync([FromServices] OpenAIService openAIService)
    {
      var models = await openAIService.GetModelsAsync();
      return Ok(models);
    }

    [HttpPost("msg")]
    public async Task<IActionResult> SendMessage([FromServices] DialogService dialogService, [FromServices] OpenAIService openAIService, AppMessageObject appMessageObject)
    {
      var dialog = dialogService.GetDialogById(appMessageObject.Message.DialogId);
      dialogService.GetDialogsForUser(appMessageObject.Message.UserId);
      var message = appMessageObject.Message;
      await openAIService.SendMessageAsync(message.Content);
      return Ok();
    }

    [HttpPost("reg")]
    public IActionResult Registration([FromServices] AuthService authService, RegistrationObject registerObject)
    {
      var user = authService.Register(registerObject.login, registerObject.password);
      return Ok(user);
    }

    [HttpPost("auth")]
    public IActionResult Authenticate([FromServices] AuthService authService, RegistrationObject registerObject)
    {
      var user = authService.Authenticate(registerObject.login, registerObject.password);
      if (user == null) return Unauthorized();
      return Ok(user);
    }
  }
}
