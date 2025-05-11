using ai_it_wiki.Services.OpenAI;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace ai_it_wiki.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class GoController : Controller
  {
    private readonly OpenAIService _openAiService;

    public GoController([FromServices] OpenAIService openAiService)
    {
      _openAiService = openAiService as OpenAIService;
    }

    public IActionResult Index()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetAiMove([FromBody] MoveRequest request)
    {
      if (request == null) return StatusCode(StatusCodes.Status400BadRequest, nameof(request));
      if (request.Attempt >= 5)
      {
        return BadRequest("ИИ не нашел достойного хода, человек побеждает!");
      }

      var aiMove = await _openAiService.GetNextMoveWithHistory(request.Board, request.Move, request.MoveHistory, request.AITactic);

      return Ok(new { aiMove });
    }


    private string GeneratePrompt(GameState state)
    {
      //var prompt = $"The top-left corner of board is indexed as ({state.FirstIndex}), the bottom-right corner is indexed as ({state.LastIndex}).\n";

      var prompt = "Cell numbering starts from 0";

      prompt += "\nPrevious turns:\n";

      //разворачиваем массив истории ходов в обратном порядке для вывода
      var history = state.BoardHistory.AsEnumerable().Reverse();

      int index = 1;
      foreach (var previousBoard in state.BoardHistory)
      {
        prompt += $"Turn {index++}:\n";
        foreach (var row in previousBoard)
        {
          prompt += string.Join(" ", row.Select(cell => cell ?? ".")) + "\n";
        }
        prompt += "\n";
      }

      //prompt += "Your next turn as O (white)?";
      return prompt;
    }

    private AiMove ParseAiMove(string response)
    {
      // Парсинг ответа ИИ, ожидая JSON формат
      try
      {
        var aiMove = JsonConvert.DeserializeObject<AiMove>(response);
        return aiMove;
      }
      catch (Exception ex)
      {
        // Обработка ошибок парсинга
        throw new Exception("Error parsing AI response", ex);
      }
    }

    public class AiMove
    {
      public int Row { get; set; }
      public int Col { get; set; }
    }

    public class GameState
    {
      public string[][] Board { get; set; }
      public string FirstIndex { get; set; }
      public string LastIndex { get; set; }
      public string CurrentStrategy { get; set; }
      public List<string[][]> BoardHistory { get; set; } // Добавление истории ходов
    }
  }

  public class MoveRequest
  {
    public Move Move { get; set; }  // Информация о ходе игрока
    public List<List<int>> Board { get; set; }  // Текущее состояние доски
    public List<MoveRecord> MoveHistory { get; set; }  // История ходов
    public int Attempt { get; set; }  // Счетчик попыток для запроса ИИ
    public string AITactic { get; set; }
  }

  public class MoveRecord
  {
    public int X { get; set; }
    public int Y { get; set; }
    public MoveActor Actor { get; set; }  // "Player" или "AI"
    public int C { get; set; }  // Цвет камня
  }


  public class Move
  {
    [JsonPropertyName("x")]
    public int X { get; set; }  // Координата X хода

    [JsonPropertyName("y")]
    public int Y { get; set; }  // Координата Y хода

    [JsonPropertyName("c")]
    public int C { get; set; }  // Цвет камня

    [JsonPropertyName("a")]
    public int A { get; set; }

    [JsonPropertyName("t")]
    public string T { get; set; }
  }

  public class MoveResponse
  {
    public Move Move { get; set; }
  }

  public enum MoveActor
  {
    Human = 0,
    AI = 1
  }
}
