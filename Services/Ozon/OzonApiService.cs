using ai_it_wiki.Options;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace ai_it_wiki.Services.Ozon
{
  public class OzonApiService : IOzonApiService
  {
    private readonly HttpClient _httpClient;
    private readonly OzonOptions _options;

    public OzonApiService(HttpClient httpClient, IOptions<OzonOptions> options)
    {
      _httpClient = httpClient;
      _options = options.Value;
      _httpClient.BaseAddress = new Uri(_options.BaseUrl);
      _httpClient.DefaultRequestHeaders.Add("Client-Id", _options.ClientId);
      _httpClient.DefaultRequestHeaders.Add("Api-Key", _options.ApiKey);
    }

    public async Task<int> GetContentRatingAsync(string sku, CancellationToken cancellationToken = default)
    {
      // Implementation placeholder
      return await Task.FromResult(0);
    }

    public async Task<string> GetProductInfoAsync(string sku, CancellationToken cancellationToken = default)
    {
      // Implementation placeholder
      return await Task.FromResult(string.Empty);
    }

    public async Task<string> GetProductDescriptionAsync(string sku, CancellationToken cancellationToken = default)
    {
      // Implementation placeholder
      return await Task.FromResult(string.Empty);
    }

    public async Task<string> ImportProductAsync(string sku, string improvedContent, CancellationToken cancellationToken = default)
    {
      // Implementation placeholder
      return await Task.FromResult(string.Empty);
    }

    public async Task WaitForImportAsync(string taskId, CancellationToken cancellationToken = default)
    {
      // Implementation placeholder
      await Task.CompletedTask;
    }
  }
}
