namespace ai_it_wiki.Internal
{
  public class KeyValidator
  {
    private readonly string _validKey;

    public KeyValidator(string validKey)
    {
      _validKey = validKey;
    }

    public bool ValidateKey(string keyToValidate)
    {
      return _validKey.Equals(keyToValidate);
    }
  }
}
