using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ai_it_wiki.Models;
using ai_it_wiki.Models.LLM;
using ai_it_wiki.Models.Ozon;
using ai_it_wiki.Services.OpenAI;
using ai_it_wiki.Services.Ozon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

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
    public class LLMOpenAPIController : OpenApiControllerBase
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
            //TODO[moderate]: использовать openAiService или удалить параметр, если он не требуется
            _logger = logger;
        }

        private static ChunkMode ParseMode(string? mode)
        {
            return mode?.ToLowerInvariant() switch
            {
                "json" or "json-array" or "array" => ChunkMode.JsonArray,
                "b64" or "base64" => ChunkMode.Base64,
                _ => ChunkMode.Raw,
            };
        }

        /// <summary>
        /// Получить схему/метаданные API и моделей, чтобы LLM знала доступные поля и порядок вызовов.
        /// </summary>
        /// <param name="part">Номер части ответа (начиная с 1). Если ответ превышает лимит токенов, контент будет возвращён по частям.</param>
        /// <param name="mode">Режим разбивки ответа на части. Поддерживает значения: 'json' (или 'json-array'), 'b64' (или 'base64'), иначе - 'raw'.</param>
        [HttpGet("schema")]
        [SwaggerOperation(
            Summary = "Схема API для LLM",
            Description = "Возвращает список эндпоинтов, параметры, а также известные поля моделей (и набор полей, допустимых в параметре fields)."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        public IActionResult GetSchema([FromQuery] int part = 1, [FromQuery] string? mode = null)
        {
            var schema = new ai_it_wiki.Models.LLM.SchemaMetadataDto
            {
                Endpoints = new List<ai_it_wiki.Models.LLM.ApiEndpointInfo>
                {
                    new()
                    {
                        Path = "/llm/products",
                        Method = "GET",
                        Summary = "Получить список товаров (объекты Ozon API)",
                        Description =
                            "Фильтры по offerIds/productIds/skus, опциональные fields, пагинация через lastId/limit.",
                        Parameters = new()
                        {
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "offerIds",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                                Description = "Фильтр по offer_id",
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "productIds",
                                In = "query",
                                Type = "array[int64]",
                                Required = false,
                                Description = "Фильтр по product_id",
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "skus",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                                Description = "Фильтр по sku",
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "lastId",
                                In = "query",
                                Type = "string",
                                Required = false,
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "limit",
                                In = "query",
                                Type = "int",
                                Required = false,
                                Default = 50,
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "fields",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                                Description = "Список полей результата",
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "part",
                                In = "query",
                                Type = "int",
                                Required = false,
                                Default = 1,
                                Description = "Часть ответа",
                            },
                        },
                    },
                    new()
                    {
                        Path = "/llm/products/info",
                        Method = "POST",
                        Summary = "Получить список товаров с подробной информацией",
                        Description =
                            "Принимает ProductInfoListRequest (product_id[]) и возвращает объекты Ozon API.",
                        Parameters = new()
                        {
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "fields",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "part",
                                In = "query",
                                Type = "int",
                                Required = false,
                                Default = 1,
                            },
                        },
                    },
                    new()
                    {
                        Path = "/llm/product/description",
                        Method = "POST",
                        Summary = "Получить описание товара",
                        Description = "Принимает { sku }, поддерживает fields и part.",
                        Parameters = new()
                        {
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "fields",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "part",
                                In = "query",
                                Type = "int",
                                Required = false,
                                Default = 1,
                            },
                        },
                    },
                    new()
                    {
                        Path = "/llm/ratings",
                        Method = "POST",
                        Summary = "Рейтинг по нескольким SKU",
                        Description =
                            "Возвращает детальный рейтинг для нескольких SKU. Поддерживает выбор полей.",
                        Parameters = new()
                        {
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "fields",
                                In = "query",
                                Type = "array[string]",
                                Required = false,
                            },
                            new ai_it_wiki.Models.LLM.ApiParameterInfo
                            {
                                Name = "part",
                                In = "query",
                                Type = "int",
                                Required = false,
                                Default = 1,
                            },
                        },
                    },
                },
                Models = new List<ai_it_wiki.Models.LLM.ModelInfo>
                {
                    new()
                    {
                        Name = nameof(ProductInfoListResponse),
                        SelectableFields = AllowedProductItemFields.ToList(),
                        Fields = typeof(ProductItem)
                            .GetProperties()
                            .Select(p => new ai_it_wiki.Models.LLM.ModelFieldInfo
                            {
                                Name = p.Name,
                                Type = p.PropertyType.Name,
                                Nullable =
                                    Nullable.GetUnderlyingType(p.PropertyType) != null
                                    || !p.PropertyType.IsValueType,
                            })
                            .ToList(),
                    },
                    new()
                    {
                        Name = nameof(ProductDescriptionResponseDto),
                        SelectableFields = AllowedProductDescriptionFields.ToList(),
                        Fields = typeof(ProductDescriptionResponseDto)
                            .GetProperties()
                            .Select(p => new ai_it_wiki.Models.LLM.ModelFieldInfo
                            {
                                Name = p.Name,
                                Type = p.PropertyType.Name,
                                Nullable =
                                    Nullable.GetUnderlyingType(p.PropertyType) != null
                                    || !p.PropertyType.IsValueType,
                            })
                            .ToList(),
                    },
                    new()
                    {
                        Name = nameof(RatingResponse),
                        SelectableFields = AllowedRatingFields.ToList(),
                        Fields = typeof(ProductRating)
                            .GetProperties()
                            .Select(p => new ai_it_wiki.Models.LLM.ModelFieldInfo
                            {
                                Name = p.Name,
                                Type = p.PropertyType.Name,
                                Nullable =
                                    Nullable.GetUnderlyingType(p.PropertyType) != null
                                    || !p.PropertyType.IsValueType,
                            })
                            .ToList(),
                    },
                },
            };

            return SplitedResponse(schema, part, ParseMode(mode));
        }

        /// <summary>
        /// Получить список товаров (объекты Ozon API)
        /// </summary>
        /// <param name="offerIds">Фильтр по offer_id (необязательно)</param>
        /// <param name="productIds">Фильтр по product_id (необязательно)</param>
        /// <param name="skus">Фильтр по sku (необязательно)</param>
        /// <param name="lastId">Пагинация: last_id (необязательно)</param>
        /// <param name="limit">Пагинация: limit (по умолчанию 1000)</param>
        /// <param name="fields">Необязательный список полей, которые нужно включить в ответ. Если не задан — вернётся полный объект.</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <param name="includeRatings">При значении <c>true</c> дополнительно запрашивает рейтинг товаров по SKU и добавляет его в результат</param>
        /// <param name="part">Номер части ответа (начиная с 1). Если ответ превышает лимит токенов, контент будет возвращён по частям.</param>
        [HttpGet("products")]
        [SwaggerOperation(
            Summary = "Получить список товаров",
            Description = "Возвращает объекты списка товаров из Ozon API",
            OperationId = "LLM_Products"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        [ProducesResponseType(typeof(ChunkedResponseDto), StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [SwaggerResponse(
            StatusCodes.Status500InternalServerError,
            "Server error",
            typeof(ErrorResponse)
        )]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductsAsync(
            [FromQuery] List<string>? offerIds,
            [FromQuery] List<long>? productIds,
            [FromQuery] List<string>? skus,
            [FromQuery] string? lastId,
            [FromQuery] int? limit,
            [FromQuery] List<string>? fields,
            CancellationToken cancellationToken,
            [FromQuery] bool includeRatings = false,
            [FromQuery] int part = 1,
            [FromQuery] string? mode = null
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
                Limit = limit ?? 1000,
            };

            try
            {
                var result = await _ozonApiService.GetProductsAsync(request, cancellationToken);

                if (includeRatings)
                {
                    var skuList = result
                        .Select(r => r.Sku)
                        .Where(s => s != 0)
                        .Distinct()
                        .ToList();

                    if (skuList.Any())
                    {
                        var ratingResponse = await _ozonApiService.GetRatingBySkusAsync(
                            new RatingRequest { Skus = skuList },
                            cancellationToken
                        );

                        var ratingDict = ratingResponse.Products
                            .ToDictionary(p => p.Sku);

                        foreach (var item in result)
                        {
                            if (ratingDict.TryGetValue(item.Sku, out var rating))
                            {
                                item.Rating = rating.Rating;
                                item.Groups = rating.Groups;
                            }
                        }
                    }
                }

                var shaped = ShapeResponse(result, fields);
                return SplitedResponse(shaped, part, ParseMode(mode));
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

        /// <summary>
        /// Получить список товаров (POST) — принимает тело запроса, совместимое с Ozon API
        /// </summary>
        /// <param name="productInfoListRequest">Объект запроса, содержащий фильтры для получения списка товаров</param>
        /// <param name="fields">Необязательный список полей, которые нужно включить в ответ. Если не задан — вернётся полный объект.</param>
        /// <param name="part">Номер части ответа (начиная с 1). Если ответ превышает лимит токенов, контент будет возвращён по частям.</param>
        /// <param name="cancellationToken">Токен отмены</param>
        [HttpPost("products/info")]
        [SwaggerOperation(
            Summary = "Получить список товаров с подробной информацией",
            Description = "Принимает ProductInfoListRequest и возвращает объекты Ozon API",
            OperationId = "LLM_ProductsInfo"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        [ProducesResponseType(typeof(ChunkedResponseDto), StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [SwaggerResponse(
            StatusCodes.Status500InternalServerError,
            "Server error",
            typeof(ErrorResponse)
        )]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductsInfoAsync(
            [FromBody] ProductInfoListRequest productInfoListRequest,
            [FromQuery] List<string>? fields,
            CancellationToken cancellationToken,
            [FromQuery] int part = 1,
            [FromQuery] string? mode = null
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
                var shaped = ShapeResponse(result, fields);
                return SplitedResponse(shaped, part, ParseMode(mode));
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

        /// <summary>
        /// Получить описание товара по SKU
        /// </summary>
        /// <param name="request">Объект запроса, содержащий SKU товара</param>
        /// <param name="fields">Необязательный список полей, включаемых в ответ (например: sku, description). Если не задан — вернётся полный объект.</param>
        /// <param name="part">Номер части ответа (начиная с 1). Если ответ превышает лимит токенов, контент будет возвращён по частям.</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        [HttpPost("product/description")]
        [SwaggerOperation(
            Summary = "Получить описание товара",
            Description = "Возвращает текстовое описание товара из Ozon API",
            OperationId = "LLM_ProductDescription"
        )]
        [SwaggerRequestExample(
            typeof(ProductDescriptionRequestDto),
            typeof(ai_it_wiki.Models.LLM.Examples.ProductDescriptionRequestExample)
        )]
        [SwaggerResponseExample(
            StatusCodes.Status200OK,
            typeof(ai_it_wiki.Models.LLM.Examples.ProductDescriptionResponseExample)
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        [ProducesResponseType(typeof(ChunkedResponseDto), StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ErrorResponseDto))]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [SwaggerResponse(
            StatusCodes.Status500InternalServerError,
            "Server error",
            typeof(ErrorResponseDto)
        )]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductDescriptionAsync(
            [FromBody] ProductDescriptionRequestDto request,
            [FromQuery] List<string>? fields,
            CancellationToken cancellationToken,
            [FromQuery] int part = 1,
            [FromQuery] string? mode = null
        )
        {
            if (string.IsNullOrWhiteSpace(request?.Sku))
            {
                return BadRequest(new ErrorResponseDto { Message = "SKU обязателен" });
            }

            try
            {
                var description = await _ozonApiService.GetProductDescriptionAsync(
                    request.Sku,
                    cancellationToken
                );

                var result = new ProductDescriptionResponseDto
                {
                    Sku = request.Sku,
                    Description = description ?? string.Empty,
                };

                var shaped = ShapeResponse(result, fields);
                return SplitedResponse(shaped, part, ParseMode(mode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении описания товара");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponseDto
                    {
                        Message = "Ошибка при получении описания товара",
                        Details = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// Генерирует строку указанной длины, содержащую случайные русские символы.
        /// </summary>
        /// <param name="request">Объект запроса, содержащий длину строки.</param>
        /// <param name="part">Часть ответа, если требуется разбить результат на части.</param>
        /// <returns>JSON-обёртка, содержащая полное содержимое или часть (если превышен лимит токенов).</returns>
        [HttpPost("length-check")]
        [Produces("application/json")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Генерация строки заданной длины",
            Description = "Метод возвращает текст, состоящий из случайных русских символов, длиной, указанной в теле запроса."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        [SwaggerResponse(400, "Недопустимая длина (меньше 1 или больше 100 000 000)")]
        public IActionResult LengthCheck(
            [FromBody] Models.LLM.LengthRequestDto request,
            int part = 1,
            [FromQuery] string? mode = null
        )
        {
            var length = request?.Length ?? 0;
            const int MaxLength = 100_000_000;

            if (length < 1 || length > MaxLength)
            {
                return BadRequest(
                    $"Недопустимая длина. Разрешённый диапазон: от 1 до {MaxLength} символов."
                );
            }

            const string russianChars = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя ";
            var sb = new StringBuilder(length);
            var rand = new Random();

            for (int i = 0; i < length; i++)
            {
                var ch = russianChars[rand.Next(russianChars.Length)];
                sb.Append(ch);
            }

            return SplitedResponse(new { value = sb.ToString() }, part, ParseMode(mode));
        }

        /// <summary>
        /// Получить рейтинг контента по набору SKU (детально)
        /// </summary>
        /// <param name="ratingRequest">Объект запроса, содержащий список SKU</param>
        /// <param name="fields">Необязательный список полей, включаемых в ответ (например: result.sku,result.rating). Если не задан — вернётся полный объект.</param>
        /// <param name="part">Номер части ответа (начиная с 1). Если ответ превышает лимит токенов, контент будет возвращён по частям.</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        [HttpPost("ratings")]
        [SwaggerOperation(
            Summary = "Рейтинг по нескольким SKU",
            Description = "Возвращает детальный рейтинг для нескольких SKU",
            OperationId = "LLM_RatingBySkus"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(ChunkedResponseDto))]
        public async Task<IActionResult> RatingBySkusAsync(
            [FromBody] RatingRequest ratingRequest,
            [FromQuery] List<string>? fields,
            CancellationToken cancellationToken,
            [FromQuery] int part = 1,
            [FromQuery] string? mode = null
        )
        {
            if (ratingRequest == null || !ratingRequest.Skus.Any())
                return BadRequest("Список SKU пуст");

            try
            {
                var result = await _ozonApiService.GetRatingBySkusAsync(
                    ratingRequest,
                    cancellationToken
                );
                var shaped = ShapeResponse(result, fields);
                return SplitedResponse(shaped, part, ParseMode(mode));
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
        /// Получить список товаров с рейтингом ниже или равным указанному.
        /// </summary>
        /// <param name="maxRating">Максимальный допустимый рейтинг</param>
        /// <param name="cancellationToken">Токен отмены</param>
        [HttpGet("products/low-rating")]
        [SwaggerOperation(
            Summary = "Товары с низким рейтингом",
            Description = "Возвращает SKU, рейтинг и группы условий товаров с рейтингом не выше заданного"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "OK", typeof(IEnumerable<object>))]
        public async Task<IActionResult> LowRatingProductsAsync(
            [FromQuery] double maxRating,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var products = await _ozonApiService.GetProductsAsync(
                    new ProductListRequest { Filter = new ProductInfoListFilter(), Limit = 1000 },
                    cancellationToken
                );

                var skus = products
                    .Where(p => p.Sku.HasValue)
                    .Select(p => p.Sku!.Value)
                    .ToList();

                if (!skus.Any())
                    return Ok(new List<object>());

                var ratings = await _ozonApiService.GetRatingBySkusAsync(
                    new RatingRequest { Skus = skus },
                    cancellationToken
                );

                var result = products
                    .Join(
                        ratings.Products,
                        p => p.Sku,
                        r => r.Sku,
                        (p, r) => new
                        {
                            sku = p.Sku,
                            rating = r.Rating,
                            groups = r.Groups,
                        }
                    )
                    .Where(x => x.rating <= maxRating)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении товаров с низким рейтингом");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorResponse("Ошибка при получении товаров с низким рейтингом", ex.Message)
                );
            }
        }

        /// <summary>
        /// Вспомогательный метод: выбор полей результата по списку fields (через запятую)
        /// Допустимо задавать вложенные поля через точку, например: "result.sku,result.rating"
        /// </summary>
        private static readonly HashSet<string> AllowedProductItemFields = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            // Support both canonical names and synonyms used in queries
            "id",
            "product_id",
            "offer_id",
            "sku",
            "name",
            "description_category_id",
            "attributes",
            "items",
            "rating",
            "groups",
        };

        private static readonly HashSet<string> AllowedProductDescriptionFields = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "sku",
            "description",
            "offer_id",
            "name",
        };

        private static readonly HashSet<string> AllowedRatingFields = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "result",
            "sku",
            "rating",
            "groups",
            "result.sku",
            "result.rating",
            "result.groups",
        };

        private static object ShapeResponse(object data, IEnumerable<string>? fields)
        {
            if (fields == null || !fields.Any())
                return data;

            var cleaned = fields.Select(f => f?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s))!;
            var requested = new HashSet<string>(cleaned!, StringComparer.OrdinalIgnoreCase);

            bool Want(params string[] tokens)
            {
                foreach (var t in tokens)
                {
                    if (requested.Contains(t))
                        return true;
                }
                return false;
            }

            // Простая реализация для известных типов
            switch (data)
            {
                case RatingResponse r when r.Products != null:
                    if (requested.Contains("result"))
                        return r; // уже всё, если просят целиком

                    var projected = r
                        .Products.Select(item => new Dictionary<string, object?>()
                        {
                            ["sku"] =
                                requested.Contains("result.sku") || requested.Contains("sku")
                                    ? item.Sku
                                    : null,
                            ["rating"] =
                                requested.Contains("result.rating") || requested.Contains("rating")
                                    ? item.Rating
                                    : null,
                            ["groups"] =
                                requested.Contains("result.groups") || requested.Contains("groups")
                                    ? item.Groups
                                    : null,
                        })
                        .Select(d =>
                            d.Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value)
                        )
                        .ToList();

                    return new Dictionary<string, object?> { ["result"] = projected };

                case ProductInfoListResponse p:
                    var items = p.Items ?? new List<ProductItem>();
                    // If fields explicitly include 'items' return full object
                    if (requested.Contains("items"))
                        return p;

                    // Determine which fields are requested (support nested tokens like "items.sku")
                    bool wantId = Want("id", "items.id", "product_id", "items.product_id");
                    bool wantOfferId = Want("offer_id", "items.offer_id");
                    bool wantSku = Want("sku", "items.sku");
                    bool wantName = Want("name", "items.name");
                    bool wantDescCatId = Want(
                        "description_category_id",
                        "items.description_category_id"
                    );
                    bool wantAttributes = Want("attributes", "items.attributes");

                    var projItems = items
                        .Select(it =>
                        {
                            var dict = new Dictionary<string, object?>();
                            if (wantId)
                                dict["id"] = it.Id; // emit canonical name
                            if (wantOfferId)
                                dict["offer_id"] = it.OfferId;
                            if (wantSku)
                                dict["sku"] = it.Sku;
                            if (wantName)
                                dict["name"] = it.Name;
                            if (wantDescCatId)
                                dict["description_category_id"] = it.DescriptionCategoryId;
                            if (wantAttributes)
                                dict["attributes"] = it; // TODO: project specific attributes if needed
                            return dict;
                        })
                        .Select(d =>
                            d.Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value)
                        )
                        .ToList();

                    var root = new Dictionary<string, object?> { ["items"] = projItems };
                    return root;
                case ProductDescriptionResponseDto pd:
                    var pdFieldSet = new HashSet<string>(
                        requested,
                        StringComparer.OrdinalIgnoreCase
                    );
                    pdFieldSet.IntersectWith(AllowedProductDescriptionFields);
                    var pdDict = new Dictionary<string, object?>();
                    if (pdFieldSet.Contains("sku"))
                        pdDict["sku"] = pd.Sku;
                    if (pdFieldSet.Contains("description"))
                        pdDict["description"] = pd.Description;
                    if (pdFieldSet.Contains("offer_id"))
                        pdDict["offer_id"] = pd.OfferId;
                    if (pdFieldSet.Contains("name"))
                        pdDict["name"] = pd.Name;
                    return pdDict.Count == 0 ? pd : pdDict;
                case IEnumerable<ProductListItem> list:
                    // for lists of lightweight items
                    var listProj = list.Select(it =>
                        {
                            var dict = new Dictionary<string, object?>();
                            if (requested.Contains("product_id") || requested.Contains("id"))
                                dict["product_id"] = it.ProductId;
                            if (requested.Contains("offer_id"))
                                dict["offer_id"] = it.OfferId;
                            if (requested.Contains("sku"))
                                dict["sku"] = it.Sku;
                            if (requested.Contains("has_fbo_stocks"))
                                dict["has_fbo_stocks"] = it.HasFboStocks;
                            if (requested.Contains("is_discounted"))
                                dict["is_discounted"] = it.IsDiscounted;
                            if (requested.Contains("rating"))
                                dict["rating"] = it.Rating;
                            if (requested.Contains("groups"))
                                dict["groups"] = it.Groups;
                            return dict;
                        })
                        .ToList();
                    return listProj;
                case RatingResponse r2 when r2.Products != null:
                    var ratingFields = new HashSet<string>(
                        requested,
                        StringComparer.OrdinalIgnoreCase
                    );
                    ratingFields.IntersectWith(AllowedRatingFields);
                    var rProj = r2
                        .Products.Select(item => new Dictionary<string, object?>
                        {
                            ["sku"] =
                                ratingFields.Contains("result.sku") || ratingFields.Contains("sku")
                                    ? item.Sku
                                    : null,
                            ["rating"] =
                                ratingFields.Contains("result.rating")
                                || ratingFields.Contains("rating")
                                    ? item.Rating
                                    : null,
                            ["groups"] =
                                ratingFields.Contains("result.groups")
                                || ratingFields.Contains("groups")
                                    ? item.Groups
                                    : null,
                        })
                        .Select(d =>
                            d.Where(kv => kv.Value != null)
                                .ToDictionary(kv => kv.Key, kv => kv.Value)
                        )
                        .ToList();
                    return new Dictionary<string, object?> { ["result"] = rProj };
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

