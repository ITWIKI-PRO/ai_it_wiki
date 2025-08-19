using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Writers;
using Microsoft.OpenApi.Extensions;

namespace ai_it_wiki.Internal
{
    public class SwaggerExporterHostedService : IHostedService
    {
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SwaggerExporterHostedService> _logger;

        public SwaggerExporterHostedService(
            ISwaggerProvider swaggerProvider,
            IWebHostEnvironment env,
            ILogger<SwaggerExporterHostedService> logger)
        {
            _swaggerProvider = swaggerProvider;
            _env = env;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var docs = new[] { "v1", "LLMOpenAPI" };
                var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(wwwroot);
                foreach (var docName in docs)
                {
                    var swaggerDoc = _swaggerProvider.GetSwagger(docName);
                    using var stringWriter = new StringWriter();
                    var jsonWriter = new OpenApiJsonWriter(stringWriter);
                    // Serialize as OpenAPI v3 (Swashbuckle default)
                    swaggerDoc.SerializeAsV3(jsonWriter);
                    var json = stringWriter.ToString();
                    var outPath = Path.Combine(wwwroot, $"openapi_{docName.ToLowerInvariant()}.json");
                    File.WriteAllText(outPath, json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to export Swagger JSON on startup");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
