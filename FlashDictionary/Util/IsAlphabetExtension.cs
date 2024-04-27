namespace FlashDictionary.Util;

public static class IsAlphabetExtension
{
  public static bool IsAlphabet(this char c)
  {
    return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
  }

  public static bool IsSmallAlphabet(this char c)
  {
    return c is >= 'a' and <= 'z';
  }
}
