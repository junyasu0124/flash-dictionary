using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Repeat
{
  /// <summary>
  /// Replaces repeated characters with a shorter form.
  /// </summary>
  /// <param name="word">A word to normalize.</param>
  /// <returns>An array of original word and normalized words.</returns>
  public static IEnumerable<string> Normalize(string word)
  {
    yield return word;

    char[] charArray = word.ToCharArray();
    char prevChar = charArray[0];
    int repeatCount = 1;
    List<char> oneTransformedChars = [];
    List<char> twoTransformedChars = [];

    for (int i = 1; i < charArray.Length; i++)
    {
      if (charArray[i] == prevChar)
      {
        repeatCount++;
      }
      else
      {
        if (repeatCount >= 3)
        {
          oneTransformedChars.Add(prevChar);
          oneTransformedChars.Add(prevChar);
          twoTransformedChars.Add(prevChar);
        }
        else
        {
          for (int j = 0; j < repeatCount; j++)
          {
            oneTransformedChars.Add(prevChar);
            twoTransformedChars.Add(prevChar);
          }
        }

        prevChar = charArray[i];
        repeatCount = 1;
      }
    }

    if (repeatCount >= 3)
    {
      oneTransformedChars.Add(prevChar);
      oneTransformedChars.Add(prevChar);
      twoTransformedChars.Add(prevChar);
    }
    else
    {
      for (int j = 0; j < repeatCount; j++)
      {
        oneTransformedChars.Add(prevChar);
        twoTransformedChars.Add(prevChar);
      }
    }

    string oneTransformed = new(CollectionsMarshal.AsSpan(oneTransformedChars));
    string twoTransformed = new(CollectionsMarshal.AsSpan(twoTransformedChars));
    if (oneTransformed != word)
    {
      yield return oneTransformed;
    }
    if (twoTransformed != word && oneTransformed != twoTransformed)
    {
      yield return twoTransformed;
    }
  }
}
