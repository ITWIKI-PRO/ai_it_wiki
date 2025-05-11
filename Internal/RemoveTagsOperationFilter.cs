using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

public class RemoveTagsOperationFilter : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Tags?.Clear();
  }
}