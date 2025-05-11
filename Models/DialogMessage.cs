using OpenAI_API.Chat;

namespace ai_it_wiki.Models
{
  public class DialogMessage
  {
    public string MessageId { get; set; }
    public string DialogId { get; set; }
    public Dialog Dialog { get; set; }
    public string Sender { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public string UserId { get; set; }

    public static explicit operator ChatMessage(DialogMessage message)
    {
      return new ChatMessage
      {
        Role = message.Sender.ToLower() == "user" ? ChatMessageRole.User : message.Sender.ToLower() == "assistant" ? ChatMessageRole.Assistant : ChatMessageRole.System,
        TextContent = message.Content,
        Name = message.UserId,
      };
    }
  }
}
