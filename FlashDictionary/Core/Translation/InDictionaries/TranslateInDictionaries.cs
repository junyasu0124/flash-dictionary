using FlashDictionary.Core.Dictionary;
using FlashDictionary.Core.Translation.InDictionaries;
using FlashDictionary.Core.Translation.InDictionaries.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashDictionary.Core.Translation;

internal static class TranslateInDictionaries
{
  public static Results? Translate(string sentence, out int searchedWordCount, out List<string> suggestions)
  {
    if (!string.IsNullOrWhiteSpace(sentence) && Dictionary.Dictionary.Positions != null)
    {
      sentence = sentence.Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ", " ");

      var sentencePreprocessed = sentence.ToLowerInvariant().RepeatNormalization().Select(x => x.SeparatorNormalization(sentence)).SelectMany(x => x).ToArray();

      var sentenceVariations = ((IEnumerable<string>)[
        .. sentencePreprocessed.Select(x => x.ConcatNormalization()).SelectMany(x => x),
        .. sentencePreprocessed.Select(x => x.ExtractNormalization().Select(y => y.ParaphraseNormalization())).SelectMany(x => x.SelectMany(y => y)),
        .. sentencePreprocessed.Select(x => x.SplitNormalization().Select(y => y.ParaphraseNormalization())).SelectMany(x => x.SelectMany(y => y)),
      ]).ToHashSet();

      HashSet<string> sentencesVariations = sentenceVariations.Select(x =>
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

        var variationWords = words.Select(x => x.BaseNormalization().Select(y => y.PronounNormalization()).SelectMany(b => b).Distinct().ToArray()).ToArray();

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
      }).SelectMany(x => x).ToHashSet();

      searchedWordCount = sentenceVariations.Count;
      suggestions = [];

      Results results = new();

      var groupedSentencesVariations = sentencesVariations.Where(x => string.IsNullOrWhiteSpace(x) == false).GroupBy(GetValueInDictionaries.ConvertKeyToKeyInPositions);

      TripleChar? originalSearchSentenceKey = GetValueInDictionaries.ConvertKeyToKeyInPositions(sentence);
      if (sentence.Length <= 2)
        originalSearchSentenceKey = null;

      List<string> references = [];
      foreach (var sentencesVariation in groupedSentencesVariations)
      {
        (Dictionary<string, List<(string Original, string[] Meaning)>>? values, List<string>? suggestions) value;
        if (originalSearchSentenceKey.HasValue && sentencesVariation.Key == originalSearchSentenceKey.Value)
        {
          value = GetValueInDictionaries.GetValue(sentencesVariation.Key, sentence.ToLowerInvariant());
          suggestions = value.suggestions!;
        }
        else
        {
          value = GetValueInDictionaries.GetValue(sentencesVariation.Key, null);
        }
        if (value.values == null || value.values.Count == 0)
          continue;

        foreach (var sentenceVariation in sentencesVariation)
        {
          if (value.values.TryGetValue(sentenceVariation, out var data))
          {
            results.AddRange(data);
            for (var i = 0; i < data.Count; i++)
            {
              foreach (var meaning in data[i].Meaning)
              {
                var referencesResult = Reference.Normalize(meaning);
                if (referencesResult != null)
                  references.AddRange(referencesResult);
              }
            }
          }
        }
      }

      foreach (var reference in references)
      {
        if (sentencesVariations.Contains(reference) == false)
        {
          var value = GetValueInDictionaries.GetValue(GetValueInDictionaries.ConvertKeyToKeyInPositions(reference), null);
          if (value.values == null || value.values.Count == 0)
            continue;

          if (value.values.TryGetValue(reference.ToLowerInvariant(), out var data))
          {
            foreach (var item in data)
            {
              if (item.Original == reference)
                results.Add(item);
            }
          }
        }
      }

      return results;
    }
    searchedWordCount = 0;
    suggestions = [];
    return null;
  }

  /// <summary>
  /// [1,2,2] => [[0,0,0][0,0,1][0,0,2][0,1,0][0,1,1][0,1,2][0,2,0][0,2,1][0,2,2][1,0,0][1,0,1][1,0,2][1,1,0][1,1,1][1,1,2][1,2,0][1,2,1][1,2,2]]
  /// </summary>
  /// <param name="nums"></param>
  /// <returns></returns>
  public static List<List<int>> Combinations(int[] nums)
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
