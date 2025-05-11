namespace ai_it_wiki.Models
{
  public class Dialog
  {
    public string DialogId { get; set; }
    public string Title { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public List<DialogMessage> Messages { get; set; }
    public DialogSettings Settings { get; set; }

    public int TokensSpent { get; set; }

    public Dialog()
    {
      Messages = new List<DialogMessage>();
    }
  }
}
