namespace ai_it_wiki.Options
{
  public class OzonOptions
  {
    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int DelayMilliseconds { get; set; } = 1000;
    public int MaxAttempts { get; set; } = 5;
  }
}
