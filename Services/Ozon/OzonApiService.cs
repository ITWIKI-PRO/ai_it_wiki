using System;
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

    public async Task<RatingBySkuResponse> GetRatingBySkusAsync(
        IEnumerable<long> skus,
        CancellationToken cancellationToken = default
    )
    {
      var payload = new RatingBySkuRequest { Skus = skus };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/product/rating-by-sku")
      {
        Content = new StringContent(
              JsonSerializer.Serialize(payload),
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
        var dto = JsonSerializer.Deserialize<RatingBySkuResponse>(content);
        if (dto?.Result == null)
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

    public async Task<int> GetContentRatingAsync(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
      var payload = new { skus = new[] { sku } };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/product/rating-by-sku")
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
            $"Ошибка при получении контент-рейтинга для SKU {sku}: {response.StatusCode} - {error}"
        );
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      try
      {
        var dto = JsonSerializer.Deserialize<RatingResponse>(content);
        if (dto?.Result == null || dto.Result.Length == 0)
        {
          throw new InvalidOperationException(
              $"Пустой результат рейтинга. Ответ: {content}"
          );
        }

        var item = dto.Result[0];
        return item.Rating;
      }
      catch (JsonException ex)
      {
        throw new InvalidOperationException(
            $"Не удалось разобрать рейтинг товара. Ответ: {content}",
            ex
        );
      }
    }

    public async Task<string> GetProductInfoAsync(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
      var payload = new { skus = new[] { sku } };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/info/list")
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
            $"Ошибка при получении информации о товаре для SKU {sku}: {response.StatusCode} - {error}"
        );
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      // Return raw content for now; higher-level parsing can be implemented when needed
      return content;
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
        if (dto?.Result == null || dto.Result.Length == 0)
        {
          return string.Empty;
        }

        return dto.Result[0].Description ?? string.Empty;
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

    public async Task<List<ProductListItem>> GetProductsAsync(ProductListRequest productInfoListRequest, CancellationToken cancellationToken = default)
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
        var env = JsonSerializer.Deserialize<OzonResponse<OzonResponseResult<ProductListItem>>>(content);
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
  }
}
