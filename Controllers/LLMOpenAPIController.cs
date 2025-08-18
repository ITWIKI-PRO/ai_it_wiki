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
using Swashbuckle.AspNetCore.Annotations;

namespace ai_it_wiki.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LLMOpenAPIController : ControllerBase
  {

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
    /// Оптимизировать рейтинг карточки товара по SKU
    /// </summary>
    /// <param name="sku">SKU товара</param>
    /// <returns>Результат оптимизации</returns>
    [HttpPost("optimize-sku")]
    [SwaggerOperation(Summary = "Оптимизировать рейтинг карточки товара по SKU", Description = "Оптимизирует карточку товара по SKU")]
    [SwaggerResponse(StatusCodes.Status200OK, "Успешно")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный SKU")]
    public async Task<IActionResult> OptimizeSku([FromQuery] string sku, CancellationToken cancellationToken)
    {
      if (string.IsNullOrWhiteSpace(sku))
      {
        return BadRequest("SKU не должен быть пустым.");
      }

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

        return Ok(new { sku, rating });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка обработки SKU {Sku}", sku);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new ErrorResponse(ex.Message, details: ex.StackTrace, sku: sku));
      }
    }

    /// <summary>
    /// Оптимизировать контент карточек товаров Ozon
    /// </summary>
    /// <param name="request">Список SKU</param>
    /// <returns>Итоговый контент-рейтинг по каждому SKU</returns>
    [HttpPost("optimize")]
    [SwaggerOperation(Summary = "Оптимизировать контент карточек товаров Ozon", Description = "Оптимизирует контент карточек товаров по списку SKU")]
    [SwaggerResponse(StatusCodes.Status200OK, "Успешно", typeof(List<object>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Список SKU не может быть пустым")]
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
          results.Add(new ErrorResponse(ex.Message, details: ex.StackTrace, sku: sku));
        }
      }

      return Ok(results);
    }
  }
}

