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
    // TODO[recommended]: добавить описание параметра cancellationToken в XML-документации
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

      var (rating, error) = await OptimizeSingleSkuAsync(sku, cancellationToken);

      if (error != null)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, error);
      }

      var result = new OptimizeResult { Sku = sku, Rating = rating };
      return Ok(result);
    }

    /// <summary>
    /// Оптимизировать контент карточек товаров Ozon
    /// </summary>
    /// <param name="request">Список SKU</param>
    /// <returns>Список результатов оптимизации по каждому SKU</returns>
    // TODO[recommended]: добавить описание параметра cancellationToken в XML-документации
    [HttpPost("optimize")]
    [SwaggerOperation(Summary = "Оптимизировать контент карточек товаров Ozon", Description = "Оптимизирует контент карточек товаров по списку SKU")]
    [SwaggerResponse(StatusCodes.Status200OK, "Успешно", typeof(List<OptimizeResult>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Список SKU не может быть пустым")]
    public async Task<IActionResult> Optimize([FromBody] OptimizeRequest request, CancellationToken cancellationToken)
    {
      if (request?.Skus == null || request.Skus.Count == 0)
      {
        return BadRequest("Список SKU не может быть пустым.");
      }

      var results = new List<OptimizeResult>();

      foreach (var sku in request.Skus)
      {
        var (rating, error) = await OptimizeSingleSkuAsync(sku, cancellationToken);
        if (error != null)
        {
          results.Add(new OptimizeResult
          {
            Sku = sku,
            Error = error
          });
        }
        else
        {
          results.Add(new OptimizeResult
          {
            Sku = sku,
            Rating = rating
          });
        }
      }

      return Ok(results);
    }

    private async Task<(int rating, ErrorResponse? error)> OptimizeSingleSkuAsync(string sku, CancellationToken ct)
    {
      try
      {
        var rating = await _ozonApiService.GetContentRatingAsync(sku, ct);

        if (rating < 100)
        {
          var info = await _ozonApiService.GetProductInfoAsync(sku, ct);
          var description = await _ozonApiService.GetProductDescriptionAsync(sku, ct);

          var improvedContent = await _openAiService.GenerateImprovedContentAsync(info, description, ct);

          var taskId = await _ozonApiService.ImportProductAsync(sku, improvedContent, ct);

          await _ozonApiService.WaitForImportAsync(taskId, ct);

          rating = await _ozonApiService.GetContentRatingAsync(sku, ct);
        }

        return (rating, null);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка обработки SKU {Sku}", sku);
        return (0, new ErrorResponse(ex.Message, details: ex.StackTrace, sku: sku));
      }
    }
  }

  // Добавьте этот класс, если его нет в вашем проекте
  public class OptimizeResult
  {
    public string Sku { get; set; }
    public int? Rating { get; set; }
    public ErrorResponse? Error { get; set; }
  }
}