using ai_it_wiki.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ai_it_wiki.Filters
{
  /// <summary>
  /// Глобальная обработка необработанных исключений.
  /// </summary>
  public class GlobalExceptionFilter : IExceptionFilter
  {
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
      _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
      _logger.LogError(context.Exception, "Необработанное исключение");

      var error = new ErrorResponse(
          context.Exception.Message,
          details: context.Exception.StackTrace);

      context.Result = new ObjectResult(error)
      {
        StatusCode = StatusCodes.Status500InternalServerError
      };
      context.ExceptionHandled = true;
    }
  }
}
