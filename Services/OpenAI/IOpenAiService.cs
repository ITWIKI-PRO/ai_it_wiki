using System.Threading.Tasks;
namespace ai_it_wiki.Services.OpenAI
{
  public interface IOpenAiService
  {
    Task<string> SendMessageAsync(string text);
  }
}
