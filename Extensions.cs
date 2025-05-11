using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ai_it_wiki
{
  public static class Extensions
  {
    private static readonly char[] MarkdownV2ReservedChars = new[]
      {
            '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
        };

    /// <summary>
    /// Экранирует зарезервированные символы для MarkdownV2.
    /// </summary>
    private static string EscapeMarkdownV2(this string input)
    {
      var sb = new StringBuilder(input.Length * 2);
      foreach (var ch in input)
      {
        if (Array.Exists(MarkdownV2ReservedChars, c => c == ch))
          sb.Append('\\').Append(ch);
        else
          sb.Append(ch);
      }
      return sb.ToString();
    }

  }



  public static class CodeDetector
  {
    private static readonly Regex TrailingParens = new Regex(
        @"\s*\([A-Za-z0-9\s]*\)\s*$",
        RegexOptions.Compiled
    );

    public static bool IsJson(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return false;
      s = s.Trim();
      if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")))
      {
        try { JsonDocument.Parse(s); return true; }
        catch (JsonException) { }
      }
      return false;
    }

    public static bool IsXml(string s)
    {
      if (string.IsNullOrWhiteSpace(s)) return false;
      s = s.Trim();
      if (s.StartsWith("<") && s.EndsWith(">"))
      {
        try { XDocument.Parse(s); return true; }
        catch { }
      }
      return false;
    }

    private static readonly string[] CodeSignatures = new[]
    {
        "{", "}", ";", "=>", "->",
        "class ", "interface ", "public ",
        "function ", "var ", "let ", "const ",
        "#include", "using ",
        "SELECT ", "INSERT ", "UPDATE ", "DELETE ",
        "<html", "<body", "<div", "</"
    };

    private static readonly Regex SqlRegex =
        new Regex(@"\b(SELECT|INSERT|UPDATE|DELETE|CREATE|DROP)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool LooksLikeCode(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return false;

      var s = TrailingParens.Replace(input, "").Trim();
      if (string.IsNullOrWhiteSpace(s))
        return false;

      if (IsJson(s) || IsXml(s))
        return true;

      if (SqlRegex.IsMatch(s))
        return true;

      if (CodeSignatures.Any(token => s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
        return true;

      if (s.Contains("\n"))
      {
        var lines = s.Split('\n');
        var indentLines = lines.Count(l => l.StartsWith("  ") || l.StartsWith("\t"));
        if (indentLines >= lines.Length / 2)
          return true;
      }

      return false;
    }
  }
}