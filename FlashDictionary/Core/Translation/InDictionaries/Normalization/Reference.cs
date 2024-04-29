using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static partial class Reference
{
  /// <summary>
  /// Splits a word by inserting spaces and hyphens in all possible ways.
  /// </summary>
  /// <param name="meaning">A meaning of searching for references.</param>
  /// <returns>An array of references. If there are no references, returns <see langword="null"/>.</returns>
  public static IEnumerable<string>? Normalize(string meaning)
  {
    var matches = ReferenceRegex().Matches(meaning);
    if (matches.Count > 0)
    {
      return matches.Where(match => match.Length > 3).Select(match => match.Value[2..^1]);
    }
    return null;
  }

  [GeneratedRegex("<→[A-Za-z-~.!? ]+?>")]
  private static partial Regex ReferenceRegex();
}
