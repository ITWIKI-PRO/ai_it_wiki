using System.Security.Cryptography;

namespace ai_it_wiki.Internal
{
  public class KeyGenerator
  {
    public static string GenerateKey(int size = 32)
    {
      using (var rng = RandomNumberGenerator.Create())
      {
        var byteArray = new byte[size];
        rng.GetBytes(byteArray);
        return Convert.ToBase64String(byteArray);
      }
    }
  }
}
