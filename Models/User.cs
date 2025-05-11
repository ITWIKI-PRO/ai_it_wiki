namespace ai_it_wiki.Models
{
  public class User
  {
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsAuthenticated { get; set; }

    public List<Dialog> Dialogs { get; set; }

    public string AccessKey { get; set; }

    public decimal Balance { get; set; }

    public User()
    {
      Dialogs = new List<Dialog>();
    }
  }

}
