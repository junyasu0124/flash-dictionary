﻿using FlashDictionary.Core.Dictionary;
using FlashDictionary.Util;
using FlashDictionary.Util.Collections;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using Windows.Storage;
using static FlashDictionary.Core.Dictionary.Dictionary;

namespace FlashDictionary.Core.Translation.InDictionaries;

file static class GetValueInDictionariesCache
{
  static GetValueInDictionariesCache()
  {
    App.Window!.Closed += (_, _) =>
    {
      DisposeFileStream();
    };
    App.Window!.Activated += (sender, e) =>
    {
      if (e.WindowActivationState == WindowActivationState.Deactivated)
        DisposeFileStream();
    };
  }
  private static void DisposeFileStream()
  {
    fileStream?.Dispose();
    fileStream = null;
    fileStreamTimer?.Dispose();
    fileStreamTimer = null;
  }

  private static readonly FixedDictionary<CharPair, Dictionary<string, List<(string Original, string[] Meaning)>>> valuesCache = new(5);
  public static bool TryGetCachedValue(CharPair key, out Dictionary<string, List<(string Original, string[] Meaning)>>? value)
  {
    return valuesCache.TryGetValue(key, out value);
  }
  public static void AddCache(CharPair key, Dictionary<string, List<(string Original, string[] Meaning)>> value)
  {
    SetValuesCacheTimer();

    valuesCache.Add(key, value);
  }
  public static void AddCacheRange(IEnumerable<KeyValuePair<CharPair, Dictionary<string, List<(string Original, string[] Meaning)>>>> items)
  {
    SetValuesCacheTimer();

    foreach (var item in items)
    {
      valuesCache.Add(item.Key, item.Value);
    }
  }

  private static Timer? valuesCacheTimer = null;
  private static void SetValuesCacheTimer()
  {
    valuesCacheTimer?.Dispose();
    valuesCacheTimer = new(60 * 1000) { AutoReset = false }; // 60s
    valuesCacheTimer.Elapsed += (_, _) =>
    {
      valuesCache.Clear();
    };
  }

  private static FileStream? fileStream = null;
  public static FileStream FileStream
  {
    get
    {
      if (fileStream == null)
      {
        fileStream = new FileStream(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, DictionaryDataFileName), FileMode.Open, FileAccess.Read);

        SetFileStreamTimer();
      }
      return fileStream;
    }
  }
  private static Timer? fileStreamTimer;
  private static void SetFileStreamTimer()
  {
    fileStreamTimer?.Dispose();
    fileStreamTimer = new(20 * 1000) { AutoReset = false }; // 20s
    fileStreamTimer.Elapsed += (_, _) =>
    {
      DisposeFileStream();
    };
  }
}

internal class GetValueInDictionaries : IDisposable
{
  private readonly FixedDictionary<CharPair, Dictionary<string, List<(string Original, string[] Meaning)>>> loaded = new(20);

  public void Dispose()
  {
    GetValueInDictionariesCache.AddCacheRange(loaded.GetLatestValues(20));
    loaded.Clear();
  }

  public Dictionary<string, List<(string Original, string[] Meaning)>> GetValue(CharPair key)
  {
    if (loaded.TryGetValue(key, out var loadedValue))
      return loadedValue;

    if (GetValueInDictionariesCache.TryGetCachedValue(key, out var cachedValue))
      return cachedValue!;

    var position = GetPositions(key);
    if (position == null)
      return [];

    var result = new Dictionary<string, List<(string Original, string[] Meaning)>>();

    for (var i = 0; i < position.Length; i++)
    {
      var data = ReadValue(position[i]);
      if (data == null)
        continue;
      foreach (var line in data.Split(DictionaryDataSeparatorBetweenWord))
      {
        var success = Split.TrySplitIntoTwo(line, DictionaryDataSeparatorBetweenOriginalAndMeaning, out string first, out string second);
        if (success)
        {
          try
          {
            var loweredKey = first.ToLower();
            if (result.TryGetValue(loweredKey, out var value) == false)
            {
              value = ([]);
              result[loweredKey] = value;
            }
            value.Add((first, second.Split(DictionaryDataSeparatorBetweenMeanings, StringSplitOptions.RemoveEmptyEntries)));
          }
          catch { }
        }
      }
    }

    GetValueInDictionariesCache.AddCache(key, result);

    return result;
  }

  /// <summary>
  /// Convert word into a <see cref="Positions"/> key format.
  /// </summary>
  /// <param name="key">A word to convert.</param>
  /// <returns>A key in <see cref="Positions"/> format.</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static CharPair ConvertKeyToKeyInPositions(string key)
  {
    if (string.IsNullOrWhiteSpace(key))
      throw new ArgumentNullException(nameof(key));
    else if (key.Length == 1)
    {
      if (key[0].IsAlphabet())
        return new(key[0]);
      else
        return new("#");
    }
    else if (key[0].IsAlphabet() == false)
    {
      return new("#");
    }
    else if (key[1].IsAlphabet())
    {
      return new(key[0], key[1]);
    }
    else
    {
      return new(key[0], '#');
    }
  }
  /// <summary>
  /// Convert word into a <see cref="Positions"/> key format.
  /// </summary>
  /// <param name="key">A word to convert.</param>
  /// <returns>A key in <see cref="Positions"/> format.</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static CharPair ConvertKeyToKeyInPositions(ReadOnlySpan<char> key)
  {
    if (key.IsEmpty)
      throw new ArgumentNullException(nameof(key));
    else if (key.Length == 1)
    {
      if (key[0].IsAlphabet())
        return new(key[0]);
      else
        return new("#");
    }
    else if (key[0].IsAlphabet() == false)
    {
      return new("#");
    }
    else if (key[1].IsAlphabet())
    {
      return new(key[0], key[1]);
    }
    else
    {
      return new(key[0], '#');
    }
  }

  private static Position[]? GetPositions(CharPair key)
  {
    if (Positions == null)
      throw new InvalidOperationException("FlashDictionary.Core.Dictionary.Dictionary.Positions is not loaded.");

    if (Positions.TryGetValue(key, out var value))
      return value;
    else
      return default;
  }

  private static string? ReadValue(Position position)
  {
    try
    {
      GetValueInDictionariesCache.FileStream.Seek(position.Offset, SeekOrigin.Begin);
      var buffer = new byte[position.Length];
      GetValueInDictionariesCache.FileStream.Read(buffer);

      return Encoding.UTF8.GetString(buffer);
    }
    catch
    {
      return null;
    }
  }
}
