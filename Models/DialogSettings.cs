namespace ai_it_wiki.Models
{
  public class DialogSettings
  {
    public string DialogId { get; set; }
    public Dialog Dialog { get; set; }
    public string Language { get; set; } = "en";
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
  }

}
