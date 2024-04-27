using FlashDictionary.Util;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlashDictionary.Core.Dictionary;

internal static class IsEdited
{
  /// <summary>
  /// Check wheather the DictionaryDataFile and the DictionaryPositionsFile were edited by someone other than the FileDictionary.
  /// </summary>
  /// <returns>Wheather the files were edited.</returns>
  public static async Task<bool> Check()
  {
    var previousDataHash = ApplicationData.Current.LocalSettings.Values[Dictionary.DictionaryDataHashSavingKey];
    if (previousDataHash is string previousDataHashString)
    {
      var previousPositionsHash = ApplicationData.Current.LocalSettings.Values[Dictionary.DictionaryPositionHashSavingKey];
      if (previousPositionsHash is string previousPositionsHashString)
      {
        var parentFolder = ApplicationData.Current.LocalCacheFolder;

        var dataFile = await parentFolder.GetFileAsync(Dictionary.DictionaryDataFileName);
        var currentDataHashString = Hash.CalculateFileHash(dataFile);
        if (previousDataHashString == currentDataHashString)
        {
          var positionsFile = await parentFolder.GetFileAsync(Dictionary.DictionaryPositionsFileName);
          var currentPositionsHashString = Hash.CalculateFileHash(positionsFile);
          if (previousPositionsHashString == currentPositionsHashString)
          {
            return false;
          }
        }
      }
    }
    return true;
  }
}
