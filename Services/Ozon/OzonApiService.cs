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
  }
}
