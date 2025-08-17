using ai_it_wiki.Controllers;
using ai_it_wiki.Data;
using ai_it_wiki.Models;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using NuGet.Packaging;

using OpenAI_API;
using OpenAI_API.Chat;

using System.Net.Http;
using System.Text;

using Tiktoken;

using static OpenAI_API.Chat.ChatMessage;
using static System.Net.Mime.MediaTypeNames;

namespace ai_it_wiki.Services.OpenAI
{
  public class OpenAIService : OpenAIAPI, IOpenAiService
  {
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(string apiKey, string proxyString, ILogger<OpenAIService> logger) : base(new APIAuthentication(apiKey))
    {
      _logger = logger;
      HttpClientFactory = new Internal.HttpProxyClientFactory(proxyString);
    }

    public async Task<string> SendMessageAsync(DialogSettings dialogSettings, string text)
    {
      _logger.LogInformation("Отправка сообщения в OpenAI: {Text}", text);
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 4000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06")
      };
      chatRequest.Messages.Add(new ChatMessage(ChatMessageRole.User, text));
      try
      {
        var answer = await Chat.CreateChatCompletionAsync(chatRequest);
        var result = answer.Choices[0].Message.TextContent;
        _logger.LogInformation("Ответ OpenAI получен");
        return result;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при обращении к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при обращении к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при обращении к OpenAI");
        throw;
      }
    }

    public async Task<string> SendMessageAsync(string text)
    {
      _logger.LogInformation("Отправка сообщения в OpenAI: {Text}", text);
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 4000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = new List<ChatMessage> { new ChatMessage(ChatMessageRole.User, text) }
      };
      try
      {
        var answer = await Chat.CreateChatCompletionAsync(chatRequest);
        var result = answer.Choices[0].Message.TextContent;
        _logger.LogInformation("Ответ OpenAI получен");
        return result;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при обращении к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при обращении к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при обращении к OpenAI");
        throw;
      }
    }

    public async void SendMessageWithStreamAsync(string text, Action<ChatResult> callBack)
    {
      _logger.LogInformation("Стриминговый запрос в OpenAI: {Text}", text);
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 8000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = new List<ChatMessage> { new ChatMessage(ChatMessageRole.User, text) }
      };
      try
      {
        await Chat.StreamChatAsync(chatRequest, callBack);
        _logger.LogInformation("Стриминговый ответ получен");
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при стриминговом запросе к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при стриминговом запросе к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при стриминговом запросе к OpenAI");
      }
    }

    public async Task<int> SendMessageWithStreamAsync(List<ChatMessage> chatMessages, Action<ChatResult> callBack, string systemMessage = "Ты полезный ассистент", int maxTokens = 8000, string model = "gpt-40")
    {
      _logger.LogInformation("Стриминговый запрос в OpenAI с {Count} сообщениями", chatMessages.Count);
      //счетаем количество токенов
      int tokens = 0;
      var encoder = ModelToEncoder.For("gpt-4o");
      foreach (var message in chatMessages)
      {
        tokens += encoder.CountTokens(message.TextContent);
      }

      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = maxTokens,
        Temperature = 0.7,
        Model = model,
        Messages = new List<ChatMessage> { new ChatMessage { Role = ChatMessageRole.System, TextContent = systemMessage + "\nФормат ответа должен соответствовать разметке 'Telegram.Bot.Types.Enums.ParseMode.Html'" } }
      };
      chatRequest.Messages.AddRange(chatMessages);
      try
      {
        await Chat.StreamChatAsync(chatRequest, callBack);
        _logger.LogInformation("Стриминговый ответ получен");
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при стриминговом запросе к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при стриминговом запросе к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при стриминговом запросе к OpenAI");
      }
      return tokens;
    }

    // Метод для преобразования состояния доски в строку
    private string ConvertBoardToString(List<List<int>> board)
    {
      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < board.Count; i++)
      {
        for (int j = 0; j < board[i].Count; j++)
        {
          sb.Append(board[i][j] + " "); // 0 - пусто, 1 - черный, 2 - белый
        }
        sb.AppendLine();
      }

      return sb.ToString();
    }

    public async Task<Move> GetNextMoveWithHistory(List<List<int>> board, Move lastMove, List<MoveRecord> moveHistory, string tactic)
    {
      _logger.LogInformation("Получение следующего хода для игры в ГО");
      // Преобразуем состояние доски в строку
      string boardState = JsonConvert.SerializeObject(board).Replace("],[", "\n").Replace("]]", string.Empty).Replace("[[", string.Empty);

      // Создаём список сообщений для чата
      var messages = new List<ChatMessage>
      {
       new ChatMessage(ChatMessageRole.System,
          @"Ты играешь партию в ГО против человека. Проведи анализ текущего состояния доски, истории диалога и тактики. Сделай ход с учетом правил и ограничений игры Го и скорректируй тактику.
           Пример JSON-ответа: {""Move"":{""x"":2,""y"":5, ""c"": 1, ""t"": ""Соперник стремится усилить свою позицию в центре и подготовить окружение черных камней слева. Также он может попытаться атаковать черные камни на правой стороне, чтобы расширить контроль над доской. Моя текущая тактика была направлена на изоляцию белой группы в левом верхнем углу, не давая им создать ""глаза"". Ходы на {1,3} для усиления давления и {2,5} для защиты снизу укрепят мои позиции и ограничат маневры белых. Моя цель — удерживать инициативу и контролировать территорию, постепенно захватывая пространство в центре.""}}. x,y - отсчет координат начинается с левого верхнего угла 0, 0; c - цвет (1 для черных, -1 для белых); t - текущая тактика."),
      };

      // Добавляем каждое сообщение из истории ходов как отдельный ChatMessage
      foreach (var moveRecord in moveHistory)
      {
        // string moveMessage = $"{{\"Move\":{{\"x\":{moveRecord.X}, {{\"x\": {moveRecord.Y}}}}}";

        string moveMessage = $"x:{moveRecord.X}, y: {moveRecord.Y}";

        ChatMessageRole sender = moveRecord.Actor == MoveActor.AI ? ChatMessageRole.Assistant : ChatMessageRole.User;

        messages.Add(new ChatMessage(sender, moveMessage));
      }

      // Добавляем текущее состояние доски и последний ход в виде нового сообщения
      string currentBoardStateMessage = $"Ход человека - {{\"Move\":{{\"x\":{lastMove.X}, \"y\": {lastMove.Y}, \"c\":-1}}\n\n\nТвой ход черными (1)";

      messages.Add(new ChatMessage(ChatMessageRole.System, $"Your current tactic:\n{tactic}\n\nBoard state:\n{boardState}"));

      messages.Add(new ChatMessage(ChatMessageRole.User, currentBoardStateMessage));


      // Создаём запрос к OpenAI с учётом предыдущих сообщений
      var chatRequest = new ChatRequest()
      {
        MaxTokens = 450,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = messages,
        ResponseFormat = ChatRequest.ResponseFormats.JsonObject
      };

      try
      {
        var response = await Chat.CreateChatCompletionAsync(chatRequest);
        var moveResponse = JsonConvert.DeserializeObject<MoveResponse>(response.Choices[0].Message.TextContent);
        return moveResponse.Move;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при запросе следующего хода к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при запросе следующего хода к OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (JsonException ex)
      {
        _logger.LogError(ex, "Ошибка разбора ответа OpenAI");
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при запросе следующего хода к OpenAI");
        throw;
      }
    }

    //метод отправки фотографии на распознование
    public async Task<string> SendPhotoAsync(string photo, string comment = null)
    {
      _logger.LogInformation("Отправка изображения в OpenAI");
      var chatRequest = new ChatRequest()
      {
        MaxTokens = 8000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06")
      };

      var imageInput = new ImageInput(photo);

      var chatMessage = new ChatMessage(ChatMessageRole.User, null, new[] { imageInput });

      if (comment != null)
      {
        chatMessage.TextContent = comment;
      }
      chatRequest.Messages = new List<ChatMessage> { chatMessage };
      try
      {
        var answer = await Chat.CreateChatCompletionAsync(chatRequest);
        var result = answer.Choices[0].Message.TextContent;
        _logger.LogInformation("Ответ OpenAI по изображению получен");
        return result;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при отправке изображения в OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при отправке изображения в OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при отправке изображения в OpenAI");
        throw;
      }
    }

    public async Task<List<OpenAI_API.Models.Model>> GetModelsAsync()
    {
      _logger.LogInformation("Получение списка моделей OpenAI");
      try
      {
        var models = Models;
        var result = await models.GetModelsAsync();
        _logger.LogInformation("Получено моделей: {Count}", result.Count);
        return result;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при получении списка моделей");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при получении списка моделей");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при получении списка моделей");
        throw;
      }
    }

    public async Task<string> SendDataToChatGPTAsync(string telemetryData)
    {
      _logger.LogInformation("Отправка телеметрии в OpenAI");
      // Формируем запрос к ChatGPT
      var chatRequest = new ChatRequest()
      {
        MaxTokens = 2000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatMessageRole.System, "You are an expert in race car tuning for Assetto Corsa. Based on current telemetry data, provide precise recommendations to improve car settings for maximum efficiency, speed, and handling. Include specific numerical adjustments for parameters such as camber, toe, spring stiffness, damper compression and rebound, brake balance, and tire parameters (pressure, temperature, and wear). Your recommendations should also account for specific sections of the track, such as corners, straights, and braking zones.\r\n\r\nYour recommendations must be concise and specific, indicating which parameters should be adjusted, by how much, and how these changes will impact performance on different sections of the track (e.g., \"increase camber by 0.5 degrees to improve cornering grip\" or \"reduce brake bias by 1% to enhance stability under braking\").\r\n Dialog with user must be on Russian language.\r\n"),
                    new ChatMessage(ChatMessageRole.User, telemetryData)
                }
      };
      string result = string.Empty;
      try
      {
        var response = await Chat.CreateChatCompletionAsync(chatRequest);

        if (response != null && response.Choices.Any())
        {
          result = response.Choices[0].Message.TextContent;
        }
        else
        {
          result = "No response from GPT";
        }
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "Сетевая ошибка при отправке телеметрии в OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        result = ex.Message;
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogError(ex, "Таймаут при отправке телеметрии в OpenAI");
        // TODO[critical]: реализовать повторные попытки и fallback-сценарии
        result = ex.Message;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Неожиданная ошибка при отправке телеметрии в OpenAI");
        result = ex.Message;
      }
      return result;
    }
  }
}