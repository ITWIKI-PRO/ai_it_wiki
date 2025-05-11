using ai_it_wiki.Data;
using ai_it_wiki.Models;
using ai_it_wiki.Services.OpenAI;
using ai_it_wiki.Services.TelegramBot;
using ai_it_wiki.Services.Youtube;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using OpenAI_API.Chat;

using System.Net;

using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ai_it_wiki.Controllers
{
  [Route("tg")]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class TgController : ControllerBase
  {
    private ApplicationDbContext _applicationDbContext;
    private TelegramBotService _botClient;
    private readonly OpenAIService _openAiService;
    private YoutubeService _youtubeService;

    public TgController([FromServices] YoutubeService youtubeService, [FromServices] ApplicationDbContext applicationDbContext, [FromServices] TelegramBotService botClient, [FromServices] OpenAIService openAiService)
    {
      _applicationDbContext = applicationDbContext;
      _botClient = botClient;
      _openAiService = openAiService;
      _youtubeService = youtubeService;
    }

    //[HttpPost("wh")]
    //public async Task<IActionResult> Webhook([FromBody] object data)
    //{
    //  var cancellationTokenSource = new CancellationTokenSource();
    //  var cancellationToken = cancellationTokenSource.Token;
    //  try
    //  {
    //    if (data == null) throw new NullReferenceException(nameof(data));
    //    string json = data.ToString();

    //    System.IO.File.WriteAllText($"wwwroot/logs/{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", json);

    //    var update = Newtonsoft.Json.JsonConvert.DeserializeObject<Update>(json);

    //    var user = _applicationDbContext.Users
    //        .Include(u => u.Dialogs)
    //        .ThenInclude(d => d.Messages)
    //        .FirstOrDefault(u => u.UserId == update.Message.From.Id.ToString());
    //    if (user == null)
    //    {
    //      user = new Models.User
    //      {
    //        UserId = update.Message.From.Id.ToString(),
    //        Username = update.Message.From.Username,
    //        Dialogs = new List<Models.Dialog> { new Models.Dialog() { DialogId = update.Message.From.Id.ToString() } }
    //      };

    //      _applicationDbContext.Users.Add(user);
    //      var changes = _applicationDbContext.SaveChanges();
    //    }

    //    if (update.Message != null && update.Message.Text != null && update.Message.Text.StartsWith("/"))
    //    {
    //      string command = update.Message.Text.ToLower();

    //      switch (command)
    //      {
    //        case "/start":
    //          // Обработка команды "Старт"
    //          break;
    //        case "/profile":
    //          // Обработка команды "Профиль"
    //          break;
    //        case "/balance":
    //          _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Ваш баланс: {user.Balance.ToString("0.00")} руб.");
    //          // Обработка команды "Баланс"
    //          break;
    //        case "/clear":
    //          await ClearCommandHandlerAsync(update.Message.Chat.Id);
    //          break;
    //        default:
    //          // Обработка неизвестной команды
    //          break;
    //      }
    //    }
    //    else
    //    {
    //      if (update.Message != null || update.EditedMessage != null)
    //      {
    //        var message = update.Message != null ? update.Message : update.EditedMessage;

    //        if (user.Balance <= 0)
    //        {
    //          await _botClient.SendTextMessageAsync(message.Chat.Id, "У вас недостаточно средств для отправки сообщения");
    //          return Ok();
    //        }

    //        _botClient.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);

    //        if (message.Text == "Пополнить баланс")
    //        {
    //          // Обработка пополнения баланса

    //        }
    //        else if (message.Text == "Токенов в диалоге")
    //        {
    //          _applicationDbContext.Entry(user).Collection(u => u.Dialogs).Load();
    //          var dialog = user.Dialogs.FirstOrDefault();
    //          if (dialog == null) await _botClient.SendTextMessageAsync(message.Chat.Id, "Диалог не найден");
    //          else await _botClient.SendTextMessageAsync(message.Chat.Id, $"Токенов в диалоге: {dialog.TokensSpent}");
    //        }
    //        else if (update.Message.Photo != null)
    //        {
    //          await PhotoMessageHandlerAsync(message);
    //        }
    //        else if (update.Message.Video != null)
    //        {
    //          // Обработка видео
    //        }
    //        else if (update.Message.Text != null)
    //        {
    //          await TextMessageHandlerAsync(user, message, cancellationTokenSource);
    //        }
    //        else if (update.Message.Audio != null)
    //        {
    //          // Обработка аудио
    //        }
    //      }
    //    }
    //  }
    //  catch (Exception exc)
    //  {
    //    cancellationTokenSource.Cancel();
    //    return Ok(exc.ToString());
    //  }
    //  cancellationTokenSource.Cancel();
    //  return Ok();
    //}

    [HttpPost("wh")]
    public async Task<IActionResult> Webhook([FromBody] object data)
    {
      var cancellationTokenSource = new CancellationTokenSource();
      var cancellationToken = cancellationTokenSource.Token;
      try
      {
        if (data == null) throw new NullReferenceException(nameof(data));
        string json = data.ToString();
        //System.IO.File.WriteAllText($"wwwroot/logs/{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", json);
        var update = Newtonsoft.Json.JsonConvert.DeserializeObject<Update>(json);
        if (update.Message != null && update.Message.Text != null)
        {
          var videoInfo = await _youtubeService.GetVideoInfo(update.Message.Text);

          if (videoInfo.Count() == 0)
          {
            await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Видео не найдено");
          }
          else
          {
            var videoName = videoInfo.FirstOrDefault().FullName;

            videoInfo = videoInfo.OrderByDescending(v => v.ContentLength).ToList();

            var clearResolutions = videoInfo.GroupBy(v => v.Resolution + v.Fps).Select(v => v.First()).ToList();

            var resolutions = clearResolutions.Where(vf => vf.Resolution >= 360).Select(v => $"{v.Resolution}p{v.Fps} ({(v.ContentLength / 1024) / 1024}mb)").ToList();

            UserActionData userActionData = new UserActionData();
            userActionData.UserId = update.Message.Chat.Id.ToString();
            userActionData.Data = update.Message.Text;
            userActionData.Id = update.Message.MessageId;
            _applicationDbContext.UserActionsData.Add(userActionData);
            _applicationDbContext.SaveChanges();

            var inlineKeyboard = new InlineKeyboardMarkup(resolutions.Select((v, i) => new[]
                        {
                            InlineKeyboardButton.WithCallbackData(v, JsonConvert.SerializeObject(new {res = v }))
                        }));

            if (resolutions.Count == 0)
            {
              await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Видео не найдено");
            }
            else
            {

              await _botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{videoName}\nВыберите качество:", replyMarkup: inlineKeyboard);

              Task.Run(new Action(() => _youtubeService.DownloadVideo(update.Message.Text, update.Message.Chat.Id, _botClient)));
            }
            //await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Видео загружается, ожидайте", replyMarkup: keyboard);
          }
        }
        else if (!string.IsNullOrEmpty(update.CallbackQuery?.Data))
        {
          var userActionData = _applicationDbContext.UserActionsData.FirstOrDefault(uad => uad.Id == update.CallbackQuery.Message.MessageId);
          if (userActionData == null)
          {
            userActionData = _applicationDbContext.UserActionsData.FirstOrDefault(uad => uad.UserId == update.CallbackQuery.From.Id.ToString());
          }
          if (userActionData != null)
          {
            //отправляем дебажное сообщение
            await _botClient.SendTextMessageAsync(update.CallbackQuery.Id, userActionData.Data + "\n" + update.CallbackQuery.Data.ToString());


            var callbackData = JsonConvert.DeserializeObject<dynamic>(update.CallbackQuery.Data);
            string res = callbackData.res;
            await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Вы выбрали:\n" + update.CallbackQuery?.Data);

            _applicationDbContext.UserActionsData.Remove(userActionData);
            _applicationDbContext.SaveChanges();
          }

        }
      }
      catch (Exception exc)
      {
        _botClient.SendTextMessageAsync(1406950293, exc.ToString());
        cancellationTokenSource.Cancel();
        return Ok(exc.ToString());
      }
      cancellationTokenSource.Cancel();
      return Ok();
    }




    //метод очистки очереди сообещний вебхука
    [HttpGet("wh/reload")]
    public async Task<IActionResult> ReloadWebhook()
    {
      try
      {
        var webHookInfo = await _botClient.GetWebhookInfoAsync();
        await _botClient.DeleteWebhookAsync(true);
        await _botClient.SetWebhookAsync(webHookInfo.Url);
      }
      catch (Exception exc)
      {
        return Ok(exc.ToString());
      }
      return Ok();
    }

    private async Task ClearCommandHandlerAsync(long id)
    {
      var user = _applicationDbContext.Users
          .Include(u => u.Dialogs)
          .ThenInclude(d => d.Messages)
          .FirstOrDefault(u => u.UserId == id.ToString());
      var dialog = user.Dialogs.FirstOrDefault();

      if (dialog == null) await _botClient.SendTextMessageAsync(id, "История отсутствует");
      else
      {
        _applicationDbContext.Dialogs.Remove(dialog);
        _applicationDbContext.SaveChanges();

        await _botClient.SendTextMessageAsync(id, "История сообщений очищена");
      }
    }

    private async Task TextMessageHandlerAsync(Models.User user, Message message, CancellationTokenSource cancellationTokenSource)
    {
      // Инициализация переменной для ответа ИИ
      string aiAnswer = string.Empty;
      // Отправка временного сообщения в чат
      Message aiMessage = null;
      string buffer = string.Empty;  // Буфер для сбора текста
      object lockObj = new object(); // Объект для блокировки
      try
      {
        //Запуск задачи для отображения действия "набор текста"

        var dialog = user.Dialogs.FirstOrDefault();
        if (dialog == null)
        {
          dialog = new Models.Dialog { DialogId = message.Chat.Id.ToString() };
          user.Dialogs.Add(dialog);
        }

        // Добавление сообщения пользователя в базу данных
        user.Dialogs.FirstOrDefault().Messages.Add(new Models.DialogMessage { DialogId = message.Chat.Id.ToString(), Timestamp = DateTime.Now, MessageId = message.Chat.Id + "_" + message.MessageId.ToString(), Content = message.Text, Sender = ChatMessageRole.User });

        try
        {
          _applicationDbContext.SaveChanges();
        }
        catch (Exception)
        {
          throw;
        }

        List<ChatMessage> chatMessages = new List<ChatMessage>();
        user.Dialogs.FirstOrDefault().Messages.ForEach(m => chatMessages.Add((ChatMessage)m));

        int skipCounter = 0;

        // Отправка сообщений в OpenAI и обработка ответа
        dialog.TokensSpent = await _openAiService.SendMessageWithStreamAsync(chatMessages,
            (e) =>
            {
              cancellationTokenSource.Cancel();
              // Добавление текста ответа ИИ
              string deltaText = e.Choices.FirstOrDefault()?.Delta.TextContent;
              if (!string.IsNullOrEmpty(deltaText))
              {
                aiAnswer += deltaText;
                lock (lockObj) // Блокируем редактирование для предотвращения параллельных вызовов
                {
                  if (aiMessage == null)
                  {
                    aiMessage = _botClient.SendTextMessageAsync(message.Chat.Id, aiAnswer, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html).Result;
                  }
                  else if (aiMessage != null)
                  {
                    if (skipCounter >= 10)
                    {
                      _botClient.EditMessageTextAsync(aiMessage.Chat.Id, aiMessage.MessageId, aiAnswer, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                      skipCounter = 0;
                    }
                    skipCounter++;
                  }
                }
                Thread.Sleep(75);
              }
            },
           model: new OpenAI_API.Models.Model("gpt-4o-2024-08-06"));

        _botClient.EditMessageTextAsync(aiMessage.Chat.Id, aiMessage.MessageId, aiAnswer, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
      }
      catch (Exception exc)
      {
        Console.Write(exc.ToString());
        cancellationTokenSource.Cancel();
        throw;
        // Логирование или другая обработка исключений
      }
      var aiDialogMessage = new Models.DialogMessage
      {
        Content = aiAnswer,
        Sender = "assistant",
        MessageId = aiMessage.Chat.Id.ToString() + "_" + aiMessage.MessageId.ToString(),
        DialogId = user.Dialogs.FirstOrDefault().DialogId,
        Timestamp = DateTime.Now
      };

      _applicationDbContext.DialogMessages.Add(aiDialogMessage);

      try
      {
        _applicationDbContext.SaveChanges();
      }
      catch (DbUpdateConcurrencyException ex)
      {
        foreach (var entry in ex.Entries)
        {
          if (entry.Entity is Dialog)
          {
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            entry.OriginalValues.SetValues(databaseValues);
          }
        }
        _applicationDbContext.SaveChanges();
      }
    }

    private async Task PhotoMessageHandlerAsync(Message message)
    {
      if (message?.Photo != null)
      {
        var photo = message.Photo.OrderByDescending(p => p.FileSize).FirstOrDefault();
        var fileId = photo.FileId;
        var file = await _botClient.GetFileAsync(fileId);
        var url = $"https://api.telegram.org/file/bot7538701155:AAEMXyL6wqgMUWmm5VEUdR9nNybFkakr57U/{file.FilePath}";
        string comment = string.IsNullOrEmpty(message.Text) ? "Опиши обьекты на изображении" : message.Text;
        var response = await _openAiService.SendPhotoAsync(url, comment);
        await _botClient.SendTextMessageAsync(message.Chat.Id, response, parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
      }
    }
  }
}
