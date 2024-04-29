using System;
using System.Collections.Generic;
using System.Text;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Concat
{
  /// <summary>
  /// Concatenates words in a sentence in all possible ways.
  /// </summary>
  /// <param name="sentence">A sentence to concatenate.</param>
  /// <returns>An array of original sentence and concatenated sentences.</returns>
  public static IEnumerable<string> Normalize(string sentence)
  {
    string[] words = sentence.Split(' ');

    if (words.Length == 1)
    {
      yield return sentence;
    }
    else
    {
      StringBuilder builder = new();
      foreach (var combination in GenerateCombinations(words.Length - 1))
      {
        for (var i = 0; i < words.Length; i++)
        {
          if (i != words.Length - 1 && combination[i])
          {
            builder.Append(words[i]);
            builder.Append(' ');
          }
          else
          {
            builder.Append(words[i]);
          }
        }
        yield return builder.ToString();
        builder.Clear();
      }
    }
  }

  private static IEnumerable<bool[]> GenerateCombinations(int n)
  {
    if (n < 1)
    {
      throw new ArgumentException("The number of words must be greater than 1.");
    }

    int maxCombinations = (1 << n) - 1;

    for (int i = maxCombinations; i >= 0; i--)
    {
      bool[] combination = new bool[n];
      for (int j = 0; j < n; j++)
      {
        if ((i & (1 << j)) != 0)
        {
          combination[j] = true;
        }
        else
        {
          combination[j] = false;
        }
      }
      yield return combination;
    }
  }
}
