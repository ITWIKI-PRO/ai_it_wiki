using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Linq;

namespace ai_it_wiki.Internal
{
  public class StripAltronPrefixFilter : IDocumentFilter
  {
    private const string Prefix = "/Altron";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
      var newPaths = new OpenApiPaths();

     
      foreach (var entry in swaggerDoc.Paths)
      {
        var oldKey = entry.Key;
        var value = entry.Value;

        // Если путь начинается с /Altron, обрезаем префикс
        var newKey = oldKey.StartsWith(Prefix)
            ? oldKey.Substring(Prefix.Length)
            : oldKey;

        // Гарантируем, что новый ключ начинается с '/'
        if (!newKey.StartsWith("/"))
          newKey = "/" + newKey;

        newPaths.Add(newKey, value);
      }

      swaggerDoc.Paths = newPaths;
    }
  }
}
