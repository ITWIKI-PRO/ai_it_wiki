using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ai_it_wiki.Models;
using ai_it_wiki.Models.Ozon;
using ai_it_wiki.Services.OpenAI;
using ai_it_wiki.Services.Ozon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace ai_it_wiki.Controllers
{
    /// <summary>
    /// Контроллер для операций с LLM/OpenAI и интеграции с Ozon API.
    /// Содержит методы для оптимизации контента карточек товаров и отдельной оптимизации по SKU.
    /// </summary>
    [Route("llm")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "LLMOpenAPI")]
    [AllowAnonymous]
    [Swashbuckle.AspNetCore.Annotations.SwaggerTag(
        "LLM OpenAPI - endpoints for optimizing Ozon product content"
    )]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class LLMOpenAPIController : Controller
    {
        private readonly IOzonApiService _ozonApiService;
        private readonly ILogger<LLMOpenAPIController> _logger;

        public LLMOpenAPIController(
            IOzonApiService ozonApiService,
            IOpenAiService openAiService,
            ILogger<LLMOpenAPIController> logger
        )
        {
            _ozonApiService = ozonApiService;
            _logger = logger;
        }

        /// <summary>
        /// Получить список товаров (объекты Ozon API)
        /// </summary>
        /// <param name="offerIds">Фильтр по offer_id (необязательно)</param>
        /// <param name="productIds">Фильтр по product_id (необязательно)</param>
        /// <param name="skus">Фильтр по sku (необязательно)</param>
        /// <param name="lastId">Пагинация: last_id (необязательно)</param>
        /// <param name="limit">Пагинация: limit (по умолчанию 50)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        [HttpGet("products")]
        [SwaggerOperation(
            Summary = "Получить список товаров",
            Description = "Возвращает объекты списка товаров из Ozon API",
            OperationId = "LLM_Products"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProductInfoListResponse))]
        [ProducesResponseType(typeof(ProductInfoListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProductsAsync(
            [FromQuery] List<string>? offerIds,
            [FromQuery] List<long>? productIds,
            [FromQuery] List<string>? skus,
            [FromQuery] string? lastId,
            [FromQuery] int? limit,
            CancellationToken cancellationToken
        )
        {
            var filter = new ProductInfoListFilter
            {
                OfferIds = offerIds ?? new List<string>(),
                ProductId = productIds ?? new List<long>(),
                Skus = skus ?? new List<string>(),
            };

            var request = new ProductListRequest
            {
                Filter = filter,
                LastId = lastId,
                Limit = limit ?? 50,
            };

            try
            {
                var result = await _ozonApiService.GetProductsAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка товаров");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("Ошибка при получении списка товаров", ex.Message)
                );
            }
        }

        //TODO[critical]: этот метод не выполняет поиск, он возвращает список товаров с подробной информацией о них
        /// <summary>
        /// Получить список товаров (POST) — принимает тело запроса, совместимое с Ozon API
        /// </summary>
        [HttpPost("products/info")]
        [SwaggerOperation(
            Summary = "Получить список товаров с подробной информацией",
            Description = "Принимает ProductInfoListRequest и возвращает объекты Ozon API",
            OperationId = "LLM_ProductsInfo"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ProductInfoListResponse))]
        [ProducesResponseType(typeof(ProductInfoListResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProductsInfoAsync(
            [FromBody] ProductInfoListRequest productInfoListRequest,
            CancellationToken cancellationToken
        )
        {
            if (productInfoListRequest.ProductId?.Count() == 0)
            {
                return BadRequest("Request body is required");
            }

            try
            {
                var result = await _ozonApiService.GetProductInfoListAsync(
                    productInfoListRequest,
                    cancellationToken
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске списка товаров");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("Ошибка при поиске списка товаров", ex.Message)
                );
            }
        }

        // /// <summary>
        // /// Получить рейтинг контента по одному SKU (детально)
        // /// </summary>
        // [HttpGet("ratings/by-sku/{sku}")]
        // [SwaggerOperation(
        //     Summary = "Рейтинг по SKU",
        //     Description = "Возвращает детальный рейтинг и группы по SKU",
        //     OperationId = "LLM_GetRatingBySku"
        // )]
        // [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(RatingBySkuResponse))]
        // public async Task<IActionResult> GetRatingBySku(
        //     [FromRoute] long sku,
        //     [FromQuery] string? fields,
        //     CancellationToken cancellationToken
        // )
        // {
        //     var response = await _ozonApiService.GetRatingBySkusAsync(
        //         new RatingRequest { Skus = new List<long> { sku } },
        //         cancellationToken
        //     );
        //     return Ok(ShapeResponse(response, fields));
        // }

        /// <summary>
        /// Получить рейтинг контента по набору SKU (детально)
        /// </summary>
        [HttpPost("ratings")]
        [SwaggerOperation(
            Summary = "Рейтинг по нескольким SKU",
            Description = "Возвращает детальный рейтинг для нескольких SKU",
            OperationId = "LLM_RatingBySkus"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(RatingResponse))]
        public async Task<IActionResult> RatingBySkusAsync(
            [FromBody] RatingRequest ratingRequest,
            //[FromQuery] string? fields,
            CancellationToken cancellationToken
        )
        {
            if (ratingRequest == null || !ratingRequest.Skus.Any())
                return BadRequest("Список SKU пуст");

            try
            {
                // Corrected the issue by removing the assignment to a variable since the method returns void.
                var result = await _ozonApiService.GetRatingBySkusAsync(
                    ratingRequest,
                    cancellationToken
                );
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рейтинга по SKU");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("Ошибка при получении рейтинга по SKU", ex.Message)
                );
            }
        }

        /// <summary>
        /// Вспомогательный метод: выбор полей результата по списку fields (через запятую)
        /// Допустимо задавать вложенные поля через точку, например: "result.sku,result.rating"
        /// </summary>
        private static object ShapeResponse(object data, string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
                return data;

            var fieldSet = new HashSet<string>(
                fields.Split(',').Select(f => f.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            // Простая реализация для известных типов
            switch (data)
            {
                case RatingResponse r when r.Products != null:
                    if (fieldSet.Contains("result"))
                        return r; // уже всё, если просят целиком

                    var projected = r
                        .Products.Select(item => new Dictionary<string, object?>()
                        {
                            ["sku"] =
                                fieldSet.Contains("result.sku") || fieldSet.Contains("sku")
                                    ? item.Sku
                                    : null,
                            ["rating"] =
                                fieldSet.Contains("result.rating") || fieldSet.Contains("rating")
                                    ? item.Rating
                                    : null,
                            ["groups"] =
                                fieldSet.Contains("result.groups") || fieldSet.Contains("groups")
                                    ? item.Groups
                                    : null,
                            // ["improve_attributes"] =
                            //     fieldSet.Contains("result.improve_attributes")
                            //     || fieldSet.Contains("improve_attributes")
                            //         ? item.ImproveAttributes
                            //         : null,
                            // ["improve_at_least"] =
                            //     fieldSet.Contains("result.improve_at_least")
                            //     || fieldSet.Contains("improve_at_least")
                            //         ? item.ImproveAtLeast
                            //         : null,
                        })
                        .Select(d =>
                            d.Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value)
                        )
                        .ToList();

                    return new Dictionary<string, object?> { ["result"] = projected };

                case ProductInfoListResponse p:
                    var items = p.Items ?? new List<ProductItem>();
                    if (fieldSet.Contains("items"))
                        return p;

                    var projItems = items
                        .Select(it => new Dictionary<string, object?>()
                        {
                            ["offer_id"] =
                                fieldSet.Contains("items.offer_id") || fieldSet.Contains("offer_id")
                                    ? it.OfferId
                                    : null,
                            ["product_id"] =
                                fieldSet.Contains("items.product_id")
                                || fieldSet.Contains("product_id")
                                    ? it.Id
                                    : null,
                            ["sku"] =
                                fieldSet.Contains("items.sku") || fieldSet.Contains("sku")
                                    ? it.Sku
                                    : null,
                            ["name"] =
                                fieldSet.Contains("items.name") || fieldSet.Contains("name")
                                    ? it.Name
                                    : null,
                            ["description_category_id"] =
                                fieldSet.Contains("items.description_category_id")
                                || fieldSet.Contains("description_category_id")
                                    ? it.DescriptionCategoryId
                                    : null,
                            ["attributes"] =
                                fieldSet.Contains("items.attributes")
                                || fieldSet.Contains("attributes")
                                    ? it
                                    : null,
                        })
                        .Select(d =>
                            d.Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value)
                        )
                        .ToList();

                    var root = new Dictionary<string, object?> { ["items"] = projItems };
                    //if (fieldSet.Contains("total")) root["total"] = p.Total;
                    //if (fieldSet.Contains("last_id")) root["last_id"] = p.LastId;
                    return root;
            }

            return data;
        }

        ///// <summary>
        ///// Оптимизировать рейтинг карточки товара по SKU
        ///// </summary>
        ///// <param name="sku">SKU товара</param>
        ///// <param name="cancellationToken">Токен отмены операции.</param>
        ///// <returns>Результат оптимизации</returns>
        //[HttpPost("optimize-sku")]
        //[SwaggerOperation(
        //    Summary = "Оптимизировать рейтинг карточки товара по SKU",
        //    Description = "Оптимизирует карточку товара по SKU",
        //    OperationId = "LLM_OptimizeSku"
        //)]
        //[SwaggerResponse(StatusCodes.Status200OK, "Успешно", typeof(OptimizeResult))]
        //[ProducesResponseType(typeof(OptimizeResult), StatusCodes.Status200OK)]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректный SKU")]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> OptimizeSku(
        //    [FromQuery] string sku,
        //    CancellationToken cancellationToken
        //)
        //{
        //    if (string.IsNullOrWhiteSpace(sku))
        //    {
        //        return BadRequest("SKU не должен быть пустым.");
        //    }

        //    var (rating, error) = await OptimizeSingleSkuAsync(sku, cancellationToken);

        //    if (error != null)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, error);
        //    }

        //    return Ok(new OptimizeResult { Sku = sku, Rating = rating });
        //}

        ///// <summary>
        ///// Оптимизировать контент карточек товаров Ozon
        ///// </summary>
        ///// <param name="request">Список SKU</param>
        ///// <param name="cancellationToken">Токен отмены операции.</param>
        ///// <returns>Итоговый контент-рейтинг по каждому SKU</returns>
        //[HttpPost("optimize")]
        //[SwaggerOperation(
        //    Summary = "Оптимизировать контент карточек товаров Ozon",
        //    Description = "Оптимизирует контент карточек товаров по списку SKU",
        //    OperationId = "LLM_OptimizeBatch"
        //)]
        //[SwaggerResponse(StatusCodes.Status200OK, "Успешно", typeof(List<OptimizeResult>))]
        //[ProducesResponseType(typeof(List<OptimizeResult>), StatusCodes.Status200OK)]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Список SKU не может быть пустым")]
        //[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> Optimize(
        //    [FromBody] OptimizeRequest request,
        //    CancellationToken cancellationToken
        //)
        //{
        //    if (request?.Skus == null || request.Skus.Count == 0)
        //    {
        //        return BadRequest("Список SKU не может быть пустым.");
        //    }

        //    var tasks = request
        //        .Skus.Select(sku => (sku, task: OptimizeSingleSkuAsync(sku, cancellationToken)))
        //        .ToList();

        //    await Task.WhenAll(tasks.Select(t => t.task));

        //    var results = new List<OptimizeResult>();

        //    foreach (var (sku, task) in tasks)
        //    {
        //        var (rating, error) = await task;
        //        if (error != null)
        //        {
        //            results.Add(
        //                new OptimizeResult
        //                {
        //                    Sku = sku,
        //                    Rating = 0,
        //                    Error = error,
        //                }
        //            );
        //        }
        //        else
        //        {
        //            results.Add(new OptimizeResult { Sku = sku, Rating = rating });
        //        }
        //    }

        //    return Ok(results);
        //}
    }
}
