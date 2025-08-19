using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

public class OpenApiControllerBase : ControllerBase
{
  public List<string> responseParts { get; private set; }

  private List<string> SplitResponse(string response, int maxSize)
  {
    List<string> parts = new List<string>();

    var gPT4OTokenizer = new Microsoft.KernelMemory.AI.OpenAI.GPT4oTokenizer();
    var tokens = gPT4OTokenizer.GetTokens(response);
    int startIndex = 0;

    while (startIndex < tokens.Count)
    {
      int endIndex = Math.Min(startIndex + maxSize, tokens.Count);
      var partTokens = tokens.Skip(startIndex).Take(endIndex - startIndex).ToList();
      var partText = string.Join(" ", partTokens);
      parts.Add(partText);
      startIndex = endIndex;
    }

    return parts;
  }


  [Produces("application/json")]
  public IActionResult SplitedResponse<T>(string json, int part)
  {
    var gPT4OTokenizer = new Microsoft.KernelMemory.AI.OpenAI.GPT4oTokenizer();

    object? dataObject = null;
    try
    {
      dataObject = JsonConvert.DeserializeObject<T>(json);
    }
    catch { }

    var tokensCount = gPT4OTokenizer.CountTokens(json);
    if (tokensCount > 7000)
    {
      responseParts = SplitResponse(json, 7000);
      if (part > responseParts.Count || part < 0)
      {
        return BadRequest("Запрашиваемая часть не существует.");
      }

      return new JsonResult(
          new
          {
            is_consequential = true,
            content = responseParts[part - 1],
            part,
            total_parts = responseParts.Count,
          }
      );
    }
    return new JsonResult(
         new
         {
           is_consequential = false,
           content = dataObject ?? json,
           part = 1,
           total_parts = 1,
         }
    );
  }

  [Produces("application/json")]
  public IActionResult SplitedResponse(string text, int part)
  {
    var gPT4OTokenizer = new Microsoft.KernelMemory.AI.OpenAI.GPT4oTokenizer();

    object? dataObject = null;
    try
    {
      dataObject = JsonConvert.DeserializeObject(text);
    }
    catch { }

    var tokensCount = gPT4OTokenizer.CountTokens(text);
    if (tokensCount > 7000)
    {
      responseParts = SplitResponse(text, 7000);
      if (part > responseParts.Count || part < 0)
      {
        return BadRequest("Запрашиваемая часть не существует.");
      }

      return new JsonResult(
          new
          {
            is_consequential = true,
            content = responseParts[part - 1],
            part,
            total_parts = responseParts.Count,
          }
      );
    }
    return new JsonResult(
         new
         {
           is_consequential = false,
           content = dataObject ?? text,
           part = 1,
           total_parts = 1,
         }
    );



  }


}
