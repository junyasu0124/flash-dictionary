using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FlashDictionary.Core.Dictionary;

internal static class SaveDictionary
{
  public static async void SaveToLocalCacheAsync(Dictionary<string, string> dictionary, string filePath)
  {
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(dictionary, Dictionary.Options);
    await File.WriteAllBytesAsync(filePath, json);
  }
}
