using ai_it_wiki.Services.Ozon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ai_it_wiki.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LLMOpenAPIController : ControllerBase
  {
    [HttpPost("optimize-sku")]
    public async Task<IActionResult> OptimizeSku([FromQuery] long sku)
    {
      var optimizer = new ProductRatingOptimizer(new OzonClientStub());
      await optimizer.OptimizeSkuAsync(sku);
      return Ok();
    }
  }
}
