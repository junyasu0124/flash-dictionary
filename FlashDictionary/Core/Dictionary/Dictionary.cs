using System.Collections.Generic;

namespace FlashDictionary.Core.Dictionary;

internal static class Dictionary
{
  public static SortedDictionary<TripleChar, Position[]?>? Positions { get; private set; } = null;
  public static void SetPositions(SortedDictionary<TripleChar, Position[]?> data)
  {
    Positions = data;
  }
  public static void SetPositions(Dictionary<TripleChar, Position[]?> data)
  {
    Positions = new(data, new TripleCharComparer());
  }


  public static string DictionaryDataFileName { get; } = "dictionary.dictdata";
  public static string DictionaryDataCacheFileName { get; } = "dictionary-cache.dictdata";
  public static string DictionaryPositionsFileName { get; } = "dictionary.dictpos";

  public static string DictionaryDataHashSavingKey { get; } = "dictionaryDataHash";
  public static string DictionaryPositionHashSavingKey { get; } = "dictionaryPositionHash";

  // Data
  public const char DictionaryDataDictionaryNameDeclaration = '\u001C'; // \u001c{Name of dictionary or nothing}\u001c
  public const char DictionaryDataSeparatorBetweenWord = '\u001E';
  public const char DictionaryDataSeparatorBetweenOriginalAndMeaning = '\u001F';
  public const char DictionaryDataSeparatorBetweenMeanings = '\u001D';

  // Positions
  // e.g. [BOF]ab:12(offset)-34(length)|56-78,ac:\u0000,...[EOF]
  public const char DictionaryPositionsNull = '\u0000';
  public const char DictionaryPositionsSeparatorBetweenKey = ',';
  public const char DictionaryPositionsSeparatorBetweenKeyAndOther = ':';
  public const char DictionaryPositionsSeparatorBetweenDictionary = '|';
  public const char DictionaryPositionSeparatorBetweenOffsetAndLength = '-';
}
