using FlashDictionary.Core.Translation.InDictionaries;
using FlashDictionary.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using static FlashDictionary.Core.Dictionary.Dictionary;

namespace FlashDictionary.Core.Dictionary;

internal static class LoadPositions
{
  /// <summary>
  /// Load Dctionary Positions file and set it to <see cref="Positions"/>.
  /// </summary>
  public static async Task LoadAsync()
  {
    var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(DictionaryPositionsFileName);
    var data = await FileIO.ReadTextAsync(file);

    SetPositions(new SortedDictionary<TripleChar, Position[]?>());
    var splitedBetweenKey = data.Split(DictionaryPositionsSeparatorBetweenKey);
    foreach (var item in splitedBetweenKey)
    {
      ProcessItem(item);
    }
  }

  private static void ProcessItem(string item)
  {
    var success = Split.TrySplitIntoTwo(item, DictionaryPositionsSeparatorBetweenKeyAndOther, out ReadOnlySpan<char> key, out string other);
    if (success)
    {
      var keyInPositions = GetValueInDictionaries.ConvertKeyToKeyInPositions(key);
      if (other[0] == DictionaryPositionsNull)
      {
        Positions![keyInPositions] = null;
      }
      else
      {
        try
        {
          Positions![keyInPositions] = other.Split(DictionaryPositionsSeparatorBetweenDictionary).Select(x =>
          {
            var success = Split.TrySplitIntoTwo(x, DictionaryPositionSeparatorBetweenOffsetAndLength, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second);
            if (!success)
            {
              throw new FormatException();
            }
            return new Position(long.Parse(first), long.Parse(second));
          }).ToArray();
        }
        catch (Exception e)
        {
        }
      }
    }
  }
}
