namespace ai_it_wiki.Internal
{
  public interface IKeyService
  {
    string GenerateKey();
    bool ValidateKey(string key, string validKey);
  }
}
