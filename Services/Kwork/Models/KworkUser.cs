using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_it_wiki.Services.Kwork.Models
{
  public class KworkUser
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string AvatarUrl { get; set; }
    //public List<Achievement> Achievements { get; set; }

    //public KworkUser()
    //{
    //  Achievements = new List<Achievement>();
    //}
  }
}
