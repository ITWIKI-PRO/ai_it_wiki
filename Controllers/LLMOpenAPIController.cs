using System.Threading.Tasks;
using ai_it_wiki.Services.OpenAI;
using Microsoft.AspNetCore.Mvc;

namespace ai_it_wiki.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LLMOpenAPIController : ControllerBase
  {
    [HttpPost("completion")]
    public async Task<ActionResult<string>> Completion([FromBody] string prompt, [FromServices] IOpenAiService openAiService)
    {
      if (string.IsNullOrWhiteSpace(prompt))
        return BadRequest("Prompt is empty");

      var result = await openAiService.SendMessageAsync(prompt);
      return Ok(result);
    }
  }
}
