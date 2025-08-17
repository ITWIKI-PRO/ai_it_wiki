using Microsoft.AspNetCore.Http;
using ai_it_wiki.Models;
using ai_it_wiki.Services.OpenAI;
using ai_it_wiki.Services.Ozon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
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
    private readonly IOzonApiService _ozonApiService;
    private readonly IOpenAiService _openAiService;
    private readonly ILogger<LLMOpenAPIController> _logger;

    public LLMOpenAPIController(IOzonApiService ozonApiService, IOpenAiService openAiService, ILogger<LLMOpenAPIController> logger)
    {
      _ozonApiService = ozonApiService;
      _openAiService = openAiService;
      _logger = logger;
    }

    /// <summary>
    /// Оптимизировать контент карточек товаров Ozon
    /// </summary>
    /// <param name="request">Список SKU</param>
    /// <returns>Итоговый контент-рейтинг по каждому SKU</returns>
    [HttpPost("optimize")]
    public async Task<IActionResult> Optimize([FromBody] OptimizeRequest request, CancellationToken cancellationToken)
    {
      if (request?.Skus == null || request.Skus.Count == 0)
      {
        return BadRequest("Список SKU не может быть пустым.");
      }

      var results = new List<object>();

      foreach (var sku in request.Skus)
      {
        try
        {
          var rating = await _ozonApiService.GetContentRatingAsync(sku, cancellationToken);

          if (rating < 100)
          {
            var info = await _ozonApiService.GetProductInfoAsync(sku, cancellationToken);
            var description = await _ozonApiService.GetProductDescriptionAsync(sku, cancellationToken);

            var improvedContent = await _openAiService.GenerateImprovedContentAsync(info, description, cancellationToken);

            var taskId = await _ozonApiService.ImportProductAsync(sku, improvedContent, cancellationToken);

            await _ozonApiService.WaitForImportAsync(taskId, cancellationToken);

            rating = await _ozonApiService.GetContentRatingAsync(sku, cancellationToken);
          }

          results.Add(new { sku, rating });
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Ошибка обработки SKU {Sku}", sku);
          results.Add(new { sku, error = ex.Message });
        }
      }

      return Ok(results);
    }
  }
}

