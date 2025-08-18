using ai_it_wiki.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ai_it_wiki.Services.Ozon
{
  public class OzonApiService : IOzonApiService
  {
    private readonly HttpClient _httpClient;
    private readonly OzonOptions _options;
    private readonly ILogger<OzonApiService> _logger;

    public OzonApiService(HttpClient httpClient, IOptions<OzonOptions> options, ILogger<OzonApiService> logger)
    {
      _httpClient = httpClient;
      _options = options.Value;
      _logger = logger;
      _httpClient.BaseAddress = new Uri(_options.BaseUrl);
      _httpClient.DefaultRequestHeaders.Add("Client-Id", _options.ClientId);
      _httpClient.DefaultRequestHeaders.Add("Api-Key", _options.ApiKey);
    }

    public async Task<int> GetContentRatingAsync(string sku, CancellationToken cancellationToken = default)
    {
      var payload = new { skus = new[] { sku } };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/product/rating-by-sku")
      {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
      };

      using var response = await _httpClient.SendAsync(request, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Ошибка при получении контент-рейтинга для SKU {sku}: {response.StatusCode} - {error}");
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      try
      {
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("result")[0].GetProperty("rating").GetInt32();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Не удалось разобрать рейтинг товара. Ответ: {content}", ex);
      }
    }

    public async Task<string> GetProductInfoAsync(string sku, CancellationToken cancellationToken = default)
    {
      var payload = new { skus = new[] { sku } };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/info/list")
      {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
      };

      using var response = await _httpClient.SendAsync(request, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Ошибка при получении информации о товаре для SKU {sku}: {response.StatusCode} - {error}");
      }

      return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetProductDescriptionAsync(string sku, CancellationToken cancellationToken = default)
    {
      var payload = new { sku };
      using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/product/info/description")
      {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
      };

      using var response = await _httpClient.SendAsync(request, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Ошибка при получении описания товара для SKU {sku}: {response.StatusCode} - {error}");
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      try
      {
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("result")[0].GetProperty("description").GetString() ?? string.Empty;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Не удалось разобрать описание товара. Ответ: {content}", ex);
      }
    }

    public async Task<string> ImportProductAsync(string sku, string improvedContent, CancellationToken cancellationToken = default)
    {
      var payload = new
      {
        items = new[]
        {
          new { offer_id = sku, description = improvedContent }
        }
      };

      using var request = new HttpRequestMessage(HttpMethod.Post, "/v3/product/import")
      {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
      };

      using var response = await _httpClient.SendAsync(request, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Ошибка при импорте товара для SKU {sku}: {response.StatusCode} - {error}");
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      try
      {
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("result").GetProperty("task_id").GetString() ?? string.Empty;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Не удалось получить идентификатор задания импорта. Ответ: {content}", ex);
      }
    }

    public async Task WaitForImportAsync(string taskId, CancellationToken cancellationToken = default)
    {
      while (true)
      {
        using var response = await _httpClient.GetAsync($"/v1/product/import/info?task_id={taskId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
          var error = await response.Content.ReadAsStringAsync(cancellationToken);
          throw new HttpRequestException($"Ошибка при проверке статуса импорта {taskId}: {response.StatusCode} - {error}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
          using var doc = JsonDocument.Parse(content);
          var status = doc.RootElement.GetProperty("result")[0].GetProperty("status").GetString();
          switch (status)
          {
            case "imported":
            case "success":
            case "processed":
              return;
            case "failed":
            case "error":
              throw new InvalidOperationException($"Импорт завершился ошибкой: {content}");
            default:
              await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
              break;
          }
        }
        catch (InvalidOperationException)
        {
          throw;
        }
        catch (Exception ex)
        {
          throw new InvalidOperationException($"Не удалось разобрать статус импорта. Ответ: {content}", ex);
        }
      }
    }

    public async Task UpdateCardAsync(string sku, CancellationToken cancellationToken = default)
    {
      try
      {
        var payload = new { sku };
        // TODO[moderate]: добавить остальные необходимые поля для обновления карточки
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/v1/product/update", content, cancellationToken);
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
