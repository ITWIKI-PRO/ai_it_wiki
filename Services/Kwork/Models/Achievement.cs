using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ai_it_wiki.Services.Kwork.Models
{
  public class Achievement
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public KworkUser User { get; set; }
  }
}
