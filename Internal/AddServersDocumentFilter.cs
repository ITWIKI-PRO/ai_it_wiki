using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Collections.Generic;

public class AddServersDocumentFilter : IDocumentFilter
{
  public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
  {
    swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer
            {
                Url = "https://it-wiki.site/altron",
            },
            new OpenApiServer
            {
                Url = "http://localhost:5000/altron",
            }
        };
  }
}
