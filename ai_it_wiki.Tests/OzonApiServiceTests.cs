using System;
using System.Net.Http;
using System.Threading.Tasks;
using ai_it_wiki.Services.Ozon;
using RichardSzalay.MockHttp;
using Xunit;
// TODO[recommended]: добавить интеграционные тесты полного цикла с тестовым аккаунтом Ozon

namespace ai_it_wiki.Tests
{
  public class OzonApiServiceTests
  {
    [Fact]
    public async Task GetSkuAsync_ReturnsContent()
    {
      var mockHttp = new MockHttpMessageHandler();
      mockHttp.Expect(HttpMethod.Get, "https://api.ozon.local/sku/1")
              .Respond("application/json", "{\"id\":1}");

      var client = mockHttp.ToHttpClient();
      client.BaseAddress = new Uri("https://api.ozon.local");

      var service = new OzonApiService(client);
      var result = await service.GetSkuAsync("1");

      Assert.Equal("{\"id\":1}", result);
      mockHttp.VerifyNoOutstandingExpectation();
    }
  }
}
