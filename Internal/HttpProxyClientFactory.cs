using System.Net;

namespace ai_it_wiki.Internal
{
  /// <summary>
  /// Фабрика для создания клиента HTTP-прокси.
  /// </summary>
  public class HttpProxyClientFactory : IHttpClientFactory
  {
    private string _proxyString;

    public HttpProxyClientFactory(string proxyString)
    {
      _proxyString = proxyString;
    }

    /// <summary>
    /// Создает экземпляр HttpClient с настройками прокси.
    /// </summary>
    /// <param name="name">Имя клиента.</param>
    /// <returns>Экземпляр HttpClient с настройками прокси.</returns>
    public HttpClient CreateClient(string name)
    {
      try
      {
        //var proxyString = builder.Build().GetSection("Proxy:String").Value;
        var login = _proxyString?.Split("@")[0].Split(":")[0];
        var password = _proxyString?.Split("@")[0].Split(":")[1];
        var address = _proxyString?.Split("@")[1].Split(":")[0];
        var port = _proxyString?.Split("@")[1].Split(":")[1];

        var proxyCredentials = new NetworkCredential(login, password);
        var proxy = new WebProxy($"{address}:{port}")
        {
          Credentials = proxyCredentials
        };
        var httpClientHandler = new HttpClientHandler()
        {
          Proxy = proxy,
          UseProxy = true
        };
        return new HttpClient(httpClientHandler);
      }
      catch
      {
        throw;
      }
    }
  }
}
