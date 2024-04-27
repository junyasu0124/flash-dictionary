using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Separator
{
  public static IEnumerable<char> Separators { get; } = ['-', '_'];

  /// <summary>
  /// Replace the separators(-, _) with a space, insert a space before each uppercase letter, and convert uppercase letters to lowercase.
  /// </summary>
  /// <param name="word">A word to normalize.</param>
  /// <param name="originalWord">A word before lowercasing.</param>
  /// <returns>An array of original word and normalized words.</returns>
  public static IEnumerable<string> Normalize(string word, string originalWord)
  {
    yield return word;

    var noSeparatorsOriginalWord = originalWord;

    foreach (char separator in Separators)
    {
      if (word.Length == 1 && word[0] == separator)
        yield break;
      if (word.Contains(separator))
      {
        yield return word.Replace(separator, ' ');
        noSeparatorsOriginalWord = noSeparatorsOriginalWord.Replace(separator, ' ');
      }
    }

    foreach (char separator in Separators)
    {
      if (word.Contains(' '))
      {
        yield return word.Replace(' ', separator);
      }
    }

    var upperCaseCount = noSeparatorsOriginalWord.Count(char.IsUpper);
    Span<char> chars = stackalloc char[noSeparatorsOriginalWord.Length + upperCaseCount - (char.IsUpper(noSeparatorsOriginalWord[0]) ? 1 : 0)];

    if (upperCaseCount > 1)
    {
      for (int i = 0, j = 0; i < chars.Length; i++, j++)
      {
        if (i == 0)
        {
          chars[0] = char.ToLower(noSeparatorsOriginalWord[0]);
          continue;
        }
        if (char.IsUpper(noSeparatorsOriginalWord[j]))
        {
          chars[i] = ' ';
          chars[i + 1] = char.ToLower(noSeparatorsOriginalWord[j]);
          i++;
        }
        else
        {
          chars[i] = noSeparatorsOriginalWord[j];
        }
      }
      yield return new(chars);
    }
  }
}
