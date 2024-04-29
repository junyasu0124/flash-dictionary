using System;
using System.Collections.Generic;
using System.Text;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Split
{
  /// <summary>
  /// Splits a word by inserting spaces and hyphens in all possible ways.
  /// </summary>
  /// <param name="sentence">A word to split.</param>
  /// <returns>An array of original word and split words.</returns>
  public static IEnumerable<string> Normalize(string sentence)
  {
    var words = sentence.Split(' ');

    var builder = new StringBuilder();

    string[][] splits = new string[words.Length][];
    for (var i = 0; i < words.Length; i++)
    {
      splits[i] = SplitPerWord(words[i]);
    }
    var combinations = TranslateInDictionaries.Combinations(GetSplitsNums(splits));
    for (var i = 0; i < combinations.Count; i++)
    {
      for (var j = 0; j < words.Length; j++)
      {
        builder.Append(splits[j][combinations[i][j]]);
        if (j < words.Length - 1)
        {
          builder.Append(' ');
        }
      }
      yield return builder.ToString();
      builder.Clear();
    }

    static int[] GetSplitsNums(string[][] items)
    {
      int[] splitsNums = new int[items.Length];
      for (var i = 0; i < items.Length; i++)
      {
        splitsNums[i] = items[i].Length - 1;
      }
      return splitsNums;
    }
  }

  private static string[] SplitPerWord(string word)
  {
    if (word.Length < 4 || word.Length > 15)
    {
      return [word];
    }

    var result = new string[word.Length - 2];
    result[0] = word;

    for (int i = 2; i < word.Length - 1; i++)
    {
      result[i - 1] = string.Concat(word.AsSpan(0, i), " ", word.AsSpan(i));
    }

    return result;
  }
}
