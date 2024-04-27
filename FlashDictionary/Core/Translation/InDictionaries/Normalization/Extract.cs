using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class Extract
{
  /// <summary>
  /// Extracts words from a sentence in all possible ways.
  /// e.g. "pick you up" => ["pick you up", "pick you", "pick up", "you up"]
  /// </summary>
  /// <param name="sentence">A sentence to extract.</param>
  /// <returns>An array of original sentence and extracted sentences.</returns>
  public static IEnumerable<string> Normalize(string sentence)
  {
    var words = sentence.Split(' ');

    switch (words.Length)
    {
      case 0:
        yield break;
      case 1:
        yield return sentence;
        yield break;
      case 2:
        yield return sentence;
        yield return words[0];
        yield return words[1];
        yield break;
      default:
        StringBuilder builder = new();
        foreach (var combination in GenerateCombinations(Enumerable.Range(0, words.Length).ToArray(), isMaxLengthFirst: true))
        {
          builder.Clear();
          for (var i = 0; i < combination.Count; i++)
          {
            builder.Append(words[combination[i]]);
            builder.Append(' ');
          }
          if (combination.Count > 1)
          {
            var @string = builder.ToString().Trim();
            yield return @string;
          }
        }
        break;
    }
  }

  /// <summary>
  /// Generates all possible combinations of numbers.  
  /// e.g. Combination([1, 2, 3]) => [[1], [2], [1, 2], [3], [1, 3], [2, 3], [1, 2, 3]]
  /// </summary>
  /// <param name="numbers">An array of numbers.</param>
  /// <param name="isMaxLengthFirst">If <see langword="true"/>, the longest combination comes first.</param>
  /// <returns>An array of combinations.</returns>
  private static List<List<int>> GenerateCombinations(int[] numbers, bool isMaxLengthFirst = false)
  {
    int n = numbers.Length;
    int totalCombinations = 1 << n;

    List<List<int>> combinations = [];

    if (isMaxLengthFirst)
    {
      var combination = Generate(totalCombinations - 1);
      if (combination.Count > 0)
      {
        combinations.Add(combination);
      }
      for (int i = 0; i < totalCombinations - 1; i++)
      {
        combination = Generate(i);
        if (combination.Count > 0)
        {
          combinations.Add(combination);
        }
      }
    }
    else
    {
      for (int i = 0; i < totalCombinations; i++)
      {
        var combination = Generate(i);
        if (combination.Count > 0)
        {
          combinations.Add(combination);
        }
      }
    }
    return combinations.Distinct().ToList();

    List<int> Generate(int i)
    {
      List<int> combinations = [];
      int? startNumber = null;
      for (int j = 0; j < n; j++)
      {
        if ((i & (1 << j)) > 0)
        {
          startNumber ??= j;
          if (j - startNumber.Value > 4)
            break;
          combinations.Add(numbers[j]);
        }
      }
      return combinations;
    }
  }
}
