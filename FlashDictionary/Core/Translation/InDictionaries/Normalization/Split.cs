using System;
using System.Collections.Generic;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Split
{
  /// <summary>
  /// Splits a word by inserting spaces and hyphens in all possible ways.
  /// </summary>
  /// <param name="word">A word to split.</param>
  /// <returns>An array of original word and split words.</returns>
  public static IEnumerable<string> Normalize(string word)
  {
    yield return word;

    if (word.Length > 15)
    {
      yield break;
    }

    for (int i = 1; i < word.Length; i++)
    {
      yield return string.Concat(word.AsSpan(0, i), " ", word.AsSpan(i));
    }
  }
}
