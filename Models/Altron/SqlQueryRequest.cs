using Swashbuckle.AspNetCore.Filters;

using System.ComponentModel.DataAnnotations;

namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Запрос на выполнение произвольного SQL-запроса.
  /// </summary>
  public class SqlQueryRequest
  {
    /// <summary>SQL-запрос, который нужно выполнить.</summary>
    [Required, MinLength(1)]
    public string Query { get; set; }

    /// <summary>
    /// Строка подключения к базе данных.
    /// Если не указана — используется <c>DefaultAltronConnection</c> из конфигурации.
    /// </summary>
    public string ConnectionString { get; set; }
  }

  public class SqlQueryRequestExample : IExamplesProvider<SqlQueryRequest>
  {
    public SqlQueryRequest GetExamples()
    {
      return new SqlQueryRequest
      {
        Query = "SHOW TABLES",
        ConnectionString = "Server=92.51.39.249;database=ai_it_wiki;uid=altron;password=333Pycek9393!;ConvertZeroDateTime=True;"
      };
    }
  }
}
