using Microsoft.AspNetCore.Mvc;
using Kwork;
using ai_it_wiki.Services.Kwork.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace ai_it_wiki.Controllers
{
  [ApiController]
  [Route("kwork")]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class KworkApiController : Controller
  {
    private readonly KworkManager _kworkApi;

    public KworkApiController([FromServices] KworkManager kworkApi)
    {
      _kworkApi = kworkApi;

    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var proposals = await _kworkApi.GetProposals();

      return View(proposals);
    }

    [HttpGet("proposals")]
    public async Task<IActionResult> GetProposals()
    {
      var proposals = await _kworkApi.GetProposals();
      return Ok(proposals);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProposals([FromBody] List<Proposal> proposals)
    {
      var result = await _kworkApi.UpdateProposals(proposals);

      //строковое представление сообщения об ошибке
      var message = $"Изменений в базе данных - {result}\n";


      if (result == 0) return Ok(message);
      return Ok();
    }
  }
}
