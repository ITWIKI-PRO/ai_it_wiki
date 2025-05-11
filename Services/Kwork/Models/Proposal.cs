using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Services.Kwork.Models
{
  public class Proposal
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [JsonPropertyName("user_id"), JsonProperty("user_id")]
    public int UserId { get; set; }

    [ForeignKey("UserId"), System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public KworkUser User { get; set; }

    [JsonPropertyName("username"), JsonProperty("username")]
    public string Username { get; set; }

    [JsonPropertyName("profile_picture"), JsonProperty("profile_picture")]
    public string ProfilePicture { get; set; }

    [JsonPropertyName("price"), JsonProperty("price")]
    public int Price { get; set; }

    //[JsonPropertyName("achievements_list"), JsonProperty("achievements_list")]
    //public List<Achievement> AchievementsList { get; set; }

    [JsonPropertyName("description"), JsonProperty("description")]
    public string Description { get; set; }

    [JsonPropertyName("category_id"), JsonProperty("category_id")]
    public int CategoryId { get; set; }

    [JsonPropertyName("title"), JsonProperty("title")]
    public string Title { get; set; }

    [JsonPropertyName("time_left"), JsonProperty("time_left")]
    public int TimeLeft { get; set; }

    [JsonPropertyName("user_active_projects_count"), JsonProperty("user_active_projects_count")]
    public int UserActiveProjectsCount { get; set; }

    [JsonPropertyName("user_hired_percent"), JsonProperty("user_hired_percent")]
    public int UserHiredPercent { get; set; }
  }
}
