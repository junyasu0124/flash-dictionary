using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Pronoun
{
  /// <summary>
  /// Converts pronouns to a more general form.
  /// </summary>
  /// <param name="word">A word to normalize.</param>
  /// <returns>An array of original word and normalized words.</returns>
  public static IEnumerable<string> Normalize(string word)
  {
    if (word.Length < 2)
    {
      yield return word;
      yield break;
    }
    else
    {
      List<string> normalized = [word];

      var oldString = word[0] switch
      {
        'm' => iOldString,
        'y' => youOldString,
        'h' => word[1] switch
        {
          'i' => heOldString,
          'e' => sheOldString,
          _ => null,
        },
        'o' => weOldString,
        't' => theyOldString,
        'i' => itOldString,
        _ => null,
      };

      if (oldString is not null)
      {
        var iterator = oldString.GetEnumerator();
        for (var i = 0; i < 4; i++)
        {
          iterator.MoveNext();
          if (iterator.Current == word)
          {
            normalized.AddRange(newStrings[i]);
          }
        }
      }

      if (word.EndsWith("'s", StringComparison.Ordinal) && word.Length > 2)
      {
        normalized.AddRange(["one's", "someone's", word[..^2]]);
      }

      foreach (var item in normalized.Distinct())
      {
        yield return item;
      }
    }
  }

  private readonly static IEnumerable<string> iOldString = [
    "my", "me", "mine", "myself"
  ];
  private readonly static IEnumerable<string> youOldString = [
    "your", "you", "yours", "yourself"
  ];
  private readonly static IEnumerable<string> heOldString = [
    "his", "him", "his", "himself",
  ];
  private readonly static IEnumerable<string> sheOldString = [
    "her", "her", "hers", "herself",
  ];
  private readonly static IEnumerable<string> weOldString = [
    "our", "us", "ours", "ourselves",
  ];
  private readonly static IEnumerable<string> theyOldString = [
    "their", "them", "theirs", "themselves",
  ];
  private readonly static IEnumerable<string> itOldString = [
    "its", "it", "its", "itself",
  ];

  private readonly static IEnumerable<string>[] newStrings = [
    ["one's", "someone's"], ["oneself"], ["one's", "someone's", "one's own", "someone's own"], ["oneself"],
  ];
}
