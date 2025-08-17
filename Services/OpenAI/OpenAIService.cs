using ai_it_wiki.Controllers;
using ai_it_wiki.Data;
using ai_it_wiki.Models;
using ai_it_wiki.Options;

using Newtonsoft.Json;

using NuGet.Packaging;

using OpenAI_API;
using OpenAI_API.Chat;
using Microsoft.Extensions.Options;

using System.Text;

using Tiktoken;

using static OpenAI_API.Chat.ChatMessage;
using static System.Net.Mime.MediaTypeNames;

namespace ai_it_wiki.Services.OpenAI
{
  public class OpenAIService : OpenAIAPI, IOpenAiService
  {
    private readonly OpenAiOptions _options;

    public OpenAIService(IOptions<OpenAiOptions> options) : base(new APIAuthentication(options.Value.ApiKey))
    {
      _options = options.Value;
      HttpClientFactory = new Internal.HttpProxyClientFactory(_options.Proxy);
    }

    public async Task<string> SendMessageAsync(DialogSettings dialogSettings, string text)
    {
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 4000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06")
      };
      chatRequest.Messages.Add(new ChatMessage(ChatMessageRole.User, text));
      var answer = await Chat.CreateChatCompletionAsync(chatRequest);
      return answer.Choices[0].Message.TextContent;
    }

    public async Task<string> SendMessageAsync(string text)
    {
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 4000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = new List<ChatMessage> { new ChatMessage(ChatMessageRole.User, text) }
      };
      var answer = await Chat.CreateChatCompletionAsync(chatRequest);
      return answer.Choices[0].Message.TextContent;
    }

    public async void SendMessageWithStreamAsync(string text, Action<ChatResult> callBack)
    {
      ChatRequest chatRequest = new ChatRequest()
      {
        MaxTokens = 8000,
        Temperature = 0.7,
        Model = new OpenAI_API.Models.Model("gpt-4o-2024-08-06"),
        Messages = new List<ChatMessage> { new ChatMessage(ChatMessageRole.User, text) }
      };

      await Chat.StreamChatAsync(chatRequest, callBack);
    }

    public async Task<int> SendMessageWithStreamAsync(List<ChatMessage> chatMessages, Action<ChatResult> callBack, string systemMessage = "Ты полезный ассистент", int maxTokens = 8000, string model = "gpt-40")
    {
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
      await Chat.StreamChatAsync(chatRequest, callBack);
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

      // Получаем ответ от OpenAI
      var response = await Chat.CreateChatCompletionAsync(chatRequest);

      try
      {
        // Парсим ответ в объект Move
        var moveResponse = JsonConvert.DeserializeObject<MoveResponse>(response.Choices[0].Message.TextContent);
        return moveResponse.Move;
      }
      catch (Exception exc)
      {
        throw new Exception($"Error parsing AI response:\n{response.Choices[0].Message.TextContent}", exc);
      }
    }

    //метод отправки фотографии на распознование
    public async Task<string> SendPhotoAsync(string photo, string comment = null)
    {
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
      var answer = await Chat.CreateChatCompletionAsync(chatRequest);
      return answer.Choices[0].Message.TextContent;
    }

    public async Task<List<OpenAI_API.Models.Model>> GetModelsAsync()
    {
      var models = Models;
      return await models.GetModelsAsync();
    }

    public async Task<string> SendDataToChatGPTAsync(string telemetryData)
    {
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
      catch (Exception ex)
      {
        result = ex.Message;
      }
      return result;
    }
  }
}