using Newtonsoft.Json.Linq;

namespace ai_it_wiki.Models
{
    public class TranscriptResult
    {
        public string Text { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    public JArray Speakers { get; internal set; }
  }
}
