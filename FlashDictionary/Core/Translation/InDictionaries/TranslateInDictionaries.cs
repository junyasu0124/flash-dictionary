using FlashDictionary.Core.Translation.InDictionaries.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashDictionary.Core.Translation;

internal static class TranslateInDictionaries
{
  public static Results? Translate(string sentence, out int searchedWordCount)
  {
    if (!string.IsNullOrWhiteSpace(sentence) && Dictionary.Dictionary.Positions != null)
    {
      sentence = sentence.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ", " ");

      var sentencePreprocessed = sentence.ToLower().RepeatNormalization().Select(x => x.SeparatorNormalization(sentence)).SelectMany(x => x).ToArray();

      var sentenceVariations = sentencePreprocessed.Select(x => x.ConcatNormalization()).SelectMany(x => x).Concat(sentencePreprocessed.Select(x => x.ExtractNormalization().Select(y => y.ParaphraseNormalization())).SelectMany(x => x.SelectMany(y => y))).Distinct().ToArray();

      var sentencesVariations = sentenceVariations.Select(x =>
      {
        List<char> usedSeparator = [];
        for (int i = 0; i < x.Length; i++)
        {
          switch (x[i])
          {
            case '-':
              usedSeparator.Add('-');
              break;
            case '_':
              usedSeparator.Add('_');
              break;
            case ' ':
              usedSeparator.Add(' ');
              break;
          }
        }

        var words = x.Split((char[])['-', '_', ' '], StringSplitOptions.None);

        var variationWords = words.Select(x => x.BaseNormalization().Select(y => y.SplitNormalization().Select(z => z.PronounNormalization()).SelectMany(a => a)).SelectMany(b => b).Distinct().ToArray()).ToArray();

        var combinations = Combinations(variationWords.Select(x => x.Length - 1).ToArray());

        StringBuilder stringBuilder = new();
        List<string> sentences = [];
        for (var i = 0; i < combinations.Count; i++)
        {
          for (int j = 0; j < words.Length; j++)
          {
            stringBuilder.Append(variationWords[j][combinations[i][j]]);
            if (j < words.Length - 1)
            {
              stringBuilder.Append(usedSeparator[j]);
            }
          }
          sentences.Add(stringBuilder.ToString());
          stringBuilder.Clear();
        }

        return sentences;
      }).SelectMany(x => x).Distinct().ToArray();

      searchedWordCount = sentenceVariations.Length;

      Results results = new();

      var groupedSentencesVariations = sentencesVariations.Where(x => string.IsNullOrWhiteSpace(x) == false).GroupBy(InDictionaries.GetValueInDictionaries.ConvertKeyToKeyInPositions);
      using (var getValue = new InDictionaries.GetValueInDictionaries())
      {
        foreach (var sentencesVariation in groupedSentencesVariations)
        {
          var dictionary = getValue.GetValue(sentencesVariation.Key);
          if (dictionary.Count == 0)
            continue;

          foreach (var sentenceVariation in sentencesVariation)
          {
            if (dictionary.TryGetValue(sentenceVariation, out var data))
            {
              results.AddItems(data);
            }
          }
        }
      }

      return results;
    }
    searchedWordCount = 0;
    return null;
  }

  private static List<List<int>> Combinations(int[] nums)
  {
    List<List<int>> combinations = [];
    GenerateCombinations(nums, 0, [], combinations);
    return combinations;
  }
  private static void GenerateCombinations(int[] nums, int index, List<int> current, List<List<int>> combinations)
  {
    if (index == nums.Length)
    {
      combinations.Add(new List<int>(current));
      return;
    }

    for (int num = 0; num <= nums[index]; num++)
    {
      current.Add(num);
      GenerateCombinations(nums, index + 1, current, combinations);
      current.RemoveAt(current.Count - 1);
    }
  }
}
