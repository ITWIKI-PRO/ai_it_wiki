using System.Net.Http;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
  /// <summary>
  /// Сервис для обращения к Ozon API.
  /// </summary>
  public class OzonApiService
  {
    private readonly HttpClient _httpClient;

    public OzonApiService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    /// <summary>
    /// Получает информацию о SKU по его идентификатору.
    /// </summary>
    /// <param name="sku">Идентификатор SKU.</param>
    /// <returns>Ответ сервиса в виде строки.</returns>
    public virtual async Task<string> GetSkuAsync(string sku)
    {
      var response = await _httpClient.GetAsync($"/sku/{sku}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStringAsync();
    }
  }
}
