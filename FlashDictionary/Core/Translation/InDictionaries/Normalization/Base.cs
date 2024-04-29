using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Base
{
  private static bool isDataLoaded = false;

  private static SortedList<string, string>? nouns;
  private static SortedList<string, string>? verbs;

  /// <summary>
  /// Load nouns and verbs inflection data.
  /// </summary>
  public static async Task LoadData()
  {
    var installedFolder = Package.Current.InstalledLocation;
    var dataFolder = await installedFolder.GetFolderAsync("Data");
    var dictionariesFolder = await dataFolder.GetFolderAsync("Dictionaries");

    var nounsFile = await dictionariesFolder.GetFileAsync("nouns.json");
    var verbsFile = await dictionariesFolder.GetFileAsync("verbs.json");

    nouns = JsonSerializer.Deserialize<SortedList<string, string>>((await FileIO.ReadTextAsync(nounsFile)).Trim());

    verbs = JsonSerializer.Deserialize<SortedList<string, string>>((await FileIO.ReadTextAsync(verbsFile)).Trim());

    isDataLoaded = true;
  }

  /// <summary>
  /// Converts plural, progressive, past, comparative forms to base form.
  /// </summary>
  /// <param name="word">A word to normalize.</param>
  /// <returns>An array of original word and normalized words.</returns>
  public static IEnumerable<string> Normalize(string word)
  {
    if (!isDataLoaded)
      throw new InvalidOperationException($"Data at {nameof(Base)} is not loaded.");

    yield return word;

    if (nouns!.TryGetValue(word, out var noun))
    {
      yield return noun;
    }
    if (verbs!.TryGetValue(word, out var verb))
    {
      yield return verb;
    }

    var newIterator = newString.GetEnumerator();
    foreach (var old in oldString)
    {
      newIterator.MoveNext();

      if (word.EndsWith(old, StringComparison.Ordinal))
      {
        var newWord = word[..^old.Length] + newIterator.Current;
        if (newWord.Length > 0)
          yield return newWord;
      }
    }
  }

  private readonly static IEnumerable<string> oldString = [
    "ies", "es", "s", "'s", "ves", "zzes", "men",
    "ing", "ing", "ying", "bbing", "dding", "gging", "lling", "mming", "nning", "pping", "rring", "tting", "zzing",
    "ed", "ed", "ied", "bbed", "dded", "gged", "nned", "lled", "mmed", "pped", "rred", "tted", "zzed",
    "er", "er", "ier", "est", "est", "iest", "dder", "ddest", "gger", "ggest", "nner", "nnest", "ppier", "ppiest", "tter", "ttest",
  ];
  private readonly static IEnumerable<string> newString = [
    "y", "", "", "", "fe", "z", "man",
    "", "e", "ie", "b", "d", "g", "l", "m", "n", "p", "r", "t", "z",
    "", "e", "y", "b", "d", "g", "n", "l", "m", "p", "r", "t", "z",
    "", "e", "y", "", "e", "y", "d", "d", "g", "g", "n", "n", "ppy", "ppy", "t", "t",
  ];
}
