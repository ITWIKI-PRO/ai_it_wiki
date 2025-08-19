using ai_it_wiki.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
    public class RetryHandler : DelegatingHandler
    {
        private readonly OzonOptions _options;
        private readonly ILogger<RetryHandler> _logger;

        public RetryHandler(IOptions<OzonOptions> options, ILogger<RetryHandler> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var attempts = 0;
            var maxAttempts = Math.Max(1, _options.MaxAttempts);
            var delayMs = Math.Max(100, _options.DelayMilliseconds);

            while (true)
            {
                attempts++;
                HttpResponseMessage? response = null;
                try
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                catch (HttpRequestException ex) when (attempts < maxAttempts)
                {
                    _logger.LogWarning(ex, "Transient error while calling Ozon (attempt {Attempt})", attempts);
                    await Task.Delay(delayMs * attempts, cancellationToken);
                    continue;
                }

                if (response == null)
                {
                    throw new InvalidOperationException("No response from inner handler");
                }

                if ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (attempts >= maxAttempts)
                    {
                        return response;
                    }

                    if (response.Headers.RetryAfter != null)
                    {
                        var wait = response.Headers.RetryAfter.Delta ?? TimeSpan.FromSeconds(5);
                        _logger.LogWarning("Received {Status}. Waiting {Wait} before retry", response.StatusCode, wait);
                        await Task.Delay(wait, cancellationToken);
                    }
                    else
                    {
                        await Task.Delay(delayMs * attempts, cancellationToken);
                    }

                    continue;
                }

                return response;
            }
        }
    }
}
