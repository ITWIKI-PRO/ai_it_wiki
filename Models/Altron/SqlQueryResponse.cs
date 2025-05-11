using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Filters;

using static DevExpress.Data.Helpers.ExpressiveSortInfo;

namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Ответ на выполнение SQL-запроса.
  /// </summary>
  public class SqlQueryResponse
  {
    /// <summary>Список строк результата запроса.</summary>
    public string Data { get; set; }
  }

  public class SqlQueryResponseExample : IExamplesProvider<SqlQueryResponse>
  {
    public SqlQueryResponse GetExamples()
    {
      var exampleRows = new List<Dictionary<string, object>>();
      exampleRows.Add(new Dictionary<string, object>
      {
        { "Id", 1 },
        { "Name", "John Doe" },
        { "Age", 40 }

      });
      exampleRows.Add(new Dictionary<string, object>
      {
        { "Id", 2 },
        { "Name", "Jane Smith" },
         { "Age", 44 }
      });
      exampleRows.Add(new Dictionary<string, object>
      {
        { "Id", 3 },
        { "Name", "Alice Johnson" },
         { "Age", 4 }
      });

      return new SqlQueryResponse
      {
        Data = JsonConvert.SerializeObject(exampleRows)
      };
    }
  }
}
