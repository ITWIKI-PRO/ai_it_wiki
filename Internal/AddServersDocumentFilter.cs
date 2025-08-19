using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Collections.Generic;

public class AddServersDocumentFilter : IDocumentFilter
{
  public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
  {
    // Use a relative server URL so Swagger UI will send requests to the same origin
    // where the UI is served, avoiding hard-coded external hosts that may return 404.
    swaggerDoc.Servers = new List<OpenApiServer>
    {
        new OpenApiServer { Url = "/" }
    };
  }
}
