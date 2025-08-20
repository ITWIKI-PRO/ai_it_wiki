using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using ai_it_wiki.Models.Ozon;
using ai_it_wiki.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ai_it_wiki.Services.Ozon
{
    public class OzonApiService : IOzonApiService
    {
        private readonly HttpClient _httpClient;
        private readonly OzonOptions _options;
        private readonly ILogger<OzonApiService> _logger;

        private class ProductInfoListEnvelope
        {
            public ProductInfoListResponse? Result { get; set; }
        }

        public async Task<RatingResponse> GetRatingBySkusAsync(
            RatingRequest ratingRequest,
            CancellationToken cancellationToken = default
        )
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/product/rating-by-sku")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(ratingRequest),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ошибка при получении рейтинга по SKU: {response.StatusCode} - {content}"
                );
            }

            try
            {
                var dto = JsonSerializer.Deserialize<RatingResponse>(content);
                if (dto?.Products?.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Пустой или некорректный ответ рейтинга. Ответ: {content}"
                    );
                }
                return dto;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось разобрать рейтинг по SKU. Ответ: {content}",
                    ex
                );
            }
        }

        public OzonApiService(
            HttpClient httpClient,
            IOptions<OzonOptions> options,
            ILogger<OzonApiService> logger
        )
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Client-Id", _options.ClientId);
            _httpClient.DefaultRequestHeaders.Add("Api-Key", _options.ApiKey);
        }

        public async Task<string> GetProductDescriptionAsync(
            string sku,
            CancellationToken cancellationToken = default
        )
        {
            var payload = new { sku };
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/v1/product/info/description"
            )
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Ошибка при получении описания товара для SKU {sku}: {response.StatusCode} - {error}"
                );
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var dto = JsonSerializer.Deserialize<DescriptionResponse>(content);
                if (dto?.Result == null)
                {
                    return string.Empty;
                }

                return dto.Result.Description ?? string.Empty;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось разобрать описание товара. Ответ: {content}",
                    ex
                );
            }
        }

        public async Task<ProductInfoListResponse> GetProductInfoListAsync(
            ProductInfoListRequest productInfoListRequest,
            CancellationToken cancellationToken = default
        )
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/info/list")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(productInfoListRequest),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ошибка при получении списка товаров: {response.StatusCode} - {content}"
                );
            }

            try
            {
                // Try deserializing with { "result": { ... } } envelope first
                var env = JsonSerializer.Deserialize<ProductInfoListResponse>(content);
                if (env?.Items != null)
                {
                    return env;
                }

                throw new InvalidOperationException(
                    $"Не удалось разобрать список товаров. Ответ: {content}"
                );
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось разобрать список товаров. Ответ: {content}",
                    ex
                );
            }
        }

        /// <summary>
        /// Возвращает словарь описаний товаров по их идентификаторам.
        /// </summary>
        /// <param name="productIds">Список идентификаторов товаров (SKU).</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task<Dictionary<long, string?>> GetProductDescriptionsAsync(
            IEnumerable<long> productIds,
            CancellationToken cancellationToken = default
        )
        {
            var request = new ProductInfoListRequest
            {
                ProductId = productIds.ToList(),
                Fields = new List<string> { "description" },
            };

            var response = await GetProductInfoListAsync(request, cancellationToken);
            return response.Items?.ToDictionary(i => i.Id, i => i.Description)
                ?? new Dictionary<long, string?>();
        }

        public async Task<List<ProductListItem>> GetProductsAsync(
            ProductListRequest productInfoListRequest,
            CancellationToken cancellationToken = default
        )
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/list")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(productInfoListRequest),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ошибка при получении списка товаров: {response.StatusCode} - {content}"
                );
            }

            try
            {
                // Try deserializing with { "result": { ... } } envelope first
                var env = JsonSerializer.Deserialize<
                    OzonResponse<OzonResponseResult<ProductListItem>>
                >(content);
                if (env?.Result.Items != null)
                {
                    return env.Result.Items.ToList();
                }

                throw new InvalidOperationException(
                    $"Не удалось разобрать список товаров. Ответ: {content}"
                );
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось разобрать список товаров. Ответ: {content}",
                    ex
                );
            }
        }

        //
        public async Task GetAttributes() { }

        public async Task<string> ImportProductAsync(
            string sku,
            string improvedContent,
            CancellationToken cancellationToken = default
        )
        {
            var payload = new
            {
                items = new[] { new { offer_id = sku, description = improvedContent } },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/import")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Ошибка при импорте товара для SKU {sku}: {response.StatusCode} - {error}"
                );
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var dto = JsonSerializer.Deserialize<ImportResponse>(content);
                if (dto?.Result == null || string.IsNullOrEmpty(dto.Result.TaskId))
                {
                    throw new InvalidOperationException(
                        $"Не удалось получить идентификатор задания импорта. Ответ: {content}"
                    );
                }

                return dto.Result.TaskId!;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось получить идентификатор задания импорта. Ответ: {content}",
                    ex
                );
            }
        }

        public async Task WaitForImportAsync(
            string taskId,
            CancellationToken cancellationToken = default
        )
        {
            var attempts = 0;
            var maxAttempts = Math.Max(1, _options.MaxAttempts);
            var delayMs = Math.Max(100, _options.DelayMilliseconds);

            while (attempts < maxAttempts)
            {
                attempts++;
                using var response = await _httpClient.GetAsync(
                    $"/v1/product/import/info?task_id={taskId}",
                    cancellationToken
                );

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (response.Headers.RetryAfter != null)
                    {
                        var wait = response.Headers.RetryAfter.Delta ?? TimeSpan.FromSeconds(5);
                        await Task.Delay(wait, cancellationToken);
                        continue;
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"Ошибка при проверке статуса импорта {taskId}: {response.StatusCode} - {error}"
                    );
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var dto = JsonSerializer.Deserialize<ImportStatusResponse>(content);
                    if (dto?.Result == null || dto.Result.Length == 0)
                    {
                        // no result yet
                        await Task.Delay(delayMs, cancellationToken);
                        continue;
                    }

                    var status = dto.Result[0].Status?.ToLowerInvariant();
                    switch (status)
                    {
                        case "imported":
                        case "success":
                        case "processed":
                            return;
                        case "failed":
                        case "error":
                            throw new InvalidOperationException(
                                $"Импорт завершился ошибкой: {content}"
                            );
                        default:
                            await Task.Delay(delayMs, cancellationToken);
                            break;
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        $"Не удалось разобрать статус импорта. Ответ: {content}",
                        ex
                    );
                }
            }

            throw new TimeoutException(
                $"Ожидание завершения импорта {taskId} превысило допустимое число попыток ({maxAttempts})."
            );
        }

        public async Task UpdateCardAsync(string sku, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new { sku };
                // TODO[moderate]: добавить остальные необходимые поля для обновления карточки
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );
                using var response = await _httpClient.PostAsync(
                    "/v1/product/update",
                    content,
                    cancellationToken
                );
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Карточка товара {Sku} успешно обновлена", sku);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении карточки товара {Sku}", sku);
                throw;
            }
        }

        #region GetAttributesAsync
        /// <summary>
        /// Возвращает описание характеристик товаров по идентификатору и видимости.
        /// Товар можно искать по offer_id, product_id или sku.
        /// </summary>
        public async Task<ProductAttributesResponse> GetAttributesAsync(
            ProductAttributesRequest request,
            CancellationToken cancellationToken = default
        )
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/v4/product/info/attributes"
            )
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                ),
            };

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ошибка при получении характеристик товара: {response.StatusCode} - {content}"
                );
            }

            try
            {
                var dto = JsonSerializer.Deserialize<ProductAttributesResponse>(content);
                return dto
                    ?? throw new InvalidOperationException(
                        $"Пустой или некорректный ответ. Ответ: {content}"
                    );
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Не удалось разобрать характеристики товара. Ответ: {content}",
                    ex
                );
            }
        }

        //Пример запроса ProductAttributesRequest:
        //    {
        //  "filter": {
        //    "product_id": [
        //      "213761435"
        //    ],
        //    "offer_id": [
        //      "testtest5"
        //    ],
        //    "sku": [
        //      "123495432"
        //    ],
        //    "visibility": "ALL"
        //  },
        //  "limit": 100,
        //  "sort_dir": "ASC"
        //}

        //Пример ответа ProductAttributesResponse:
        //{
        //  "result": [
        //    {
        //      "id": 213761435,
        //      "barcode": "",
        //      "barcodes": [
        //        "123124123",
        //        "123342455"
        //      ],
        //      "name": "Пленка защитная для Xiaomi Redmi Note 10 Pro 5G",
        //      "offer_id": "21470",
        //      "type_id": 124572394,
        //      "height": 10,
        //      "depth": 210,
        //      "width": 140,
        //      "dimension_unit": "mm",
        //      "weight": 50,
        //      "weight_unit": "g",
        //      "primary_image": "https://cdn1.ozone.ru/s3/multimedia-4/6804736960.jpg",
        //      "sku": 423434534,
        //      "model_info": {
        //        "model_id": 43445453,
        //        "count": 4
        //      },
        //      "images": [
        //        "https://cdn1.ozone.ru/s3/multimedia-4/6804736960.jpg",
        //        "https://cdn1.ozone.ru/s3/multimedia-j/6835412647.jpg"
        //      ],
        //      "pdf_list": [],
        //      "attributes": [
        //        {
        //          "id": 5219,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //              "dictionary_value_id": 970718176,
        //              "value": "универсальный"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 11051,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 970736931,
        //              "value": "Прозрачный"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 10100,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "false"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 11794,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 970860783,
        //              "value": "safe"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 9048,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "Пленка защитная для Xiaomi Redmi Note 10 Pro 5G"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 5076,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 39638,
        //              "value": "Xiaomi"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 9024,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "21470"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 10015,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "false"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 85,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 971034861,
        //              "value": "Brand"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 9461,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 349824787,
        //              "value": "Защитная пленка для смартфона"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 4180,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "Пленка защитная для Xiaomi Redmi Note 10 Pro 5G"
        //            }
        //          ]
        //        },
        //        {
        //  "id": 4191,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 0,
        //              "value": "Пленка предназначена для модели Xiaomi Redmi Note 10 Pro 5G. Защитная гидрогелевая пленка обеспечит защиту вашего смартфона от царапин, пыли, сколов и потертостей."
        //            }
        //          ]
        //        },
        //        {
        //  "id": 8229,
        //          "complex_id": 0,
        //          "values": [
        //            {
        //    "dictionary_value_id": 91521,
        //              "value": "Защитная пленка"
        //            }
        //          ]
        //        }
        //      ],
        //      "attributes_with_defaults": [
        //        5435,
        //        3452
        //      ],
        //      "complex_attributes": [],
        //      "color_image": "",
        //      "description_category_id": 71107562
        //    }
        //  ],
        //  "total": 1,
        //  "last_id": "onVsfA=="
        //}

        #endregion
    }

    #region GetAttributesAsync models


    #endregion
}
