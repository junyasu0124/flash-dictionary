using System.Collections.Generic;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static class NormalizationExtensions
{
  /// <inheritdoc cref="Base.Normalize(string)"/>
  public static IEnumerable<string> BaseNormalization(this string word)
  {
    return Base.Normalize(word);
  }

  /// <inheritdoc cref="Pronoun.Normalize(string)"/>
  public static IEnumerable<string> PronounNormalization(this string word)
  {
    return Pronoun.Normalize(word);
  }

  /// <inheritdoc cref="Repeat.Normalize(string)"/>
  public static IEnumerable<string> RepeatNormalization(this string word)
  {
    return Repeat.Normalize(word);
  }

  /// <inheritdoc cref="Separator.Normalize(string, string)"/>
  public static IEnumerable<string> SeparatorNormalization(this string word, string originalWord)
  {
    return Separator.Normalize(word, originalWord);
  }

  /// <inheritdoc cref="Split.Normalize(string)"/>
  public static IEnumerable<string> SplitNormalization(this string word)
  {
    return Split.Normalize(word);
  }

  /// <inheritdoc cref="Paraphrase.Normalize(string)"/>
  public static IEnumerable<string> ParaphraseNormalization(this string sentence)
  {
    return Paraphrase.Normalize(sentence);
  }

  /// <inheritdoc cref="Concat.Normalize(string)"/>
  public static IEnumerable<string> ConcatNormalization(this string sentence)
  {
    return Concat.Normalize(sentence);
  }

  /// <inheritdoc cref="Extract.Normalize(string)"/>
  public static IEnumerable<string> ExtractNormalization(this string sentence)
  {
    return Extract.Normalize(sentence);
  }
}
