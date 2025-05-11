namespace ai_it_wiki.Internal
{
  public class KeyService : IKeyService
  {
    public string GenerateKey()
    {
      return KeyGenerator.GenerateKey();
    }

    public bool ValidateKey(string key, string validKey)
    {
      var validator = new KeyValidator(validKey);
      return validator.ValidateKey(key);
    }
  }
}
