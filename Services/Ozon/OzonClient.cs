using ai_it_wiki.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
  public class OzonClient : IOzonClient
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<OzonClient> _logger;

    public OzonClient(HttpClient httpClient, IOptions<OzonOptions> options, ILogger<OzonClient> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
      var opt = options.Value;
      _httpClient.BaseAddress = new Uri(opt.BaseUrl);
      _httpClient.DefaultRequestHeaders.Add("Client-Id", opt.ClientId);
      _httpClient.DefaultRequestHeaders.Add("Api-Key", opt.ApiKey);
    }

    public async Task<int> GetRatingAsync(string sku)
    {
      try
      {
        var payload = new { sku };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/v1/product/info/rating", content);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        if (doc.RootElement.TryGetProperty("result", out var result) &&
            result.TryGetProperty("rating", out var rating))
        {
          return rating.GetInt32();
        }

        _logger.LogWarning("Не удалось получить рейтинг из ответа Ozon API для SKU {Sku}", sku);
        return 0;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при получении рейтинга для SKU {Sku}", sku);
        return 0;
      }
    }

    public async Task UpdateCardAsync(string sku)
    {
      try
      {
        var payload = new { sku };
        // TODO[moderate]: добавить остальные необходимые поля для обновления карточки
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/v1/product/update", content);
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
