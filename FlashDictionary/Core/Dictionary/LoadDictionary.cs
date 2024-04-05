using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashDictionary.Core.Dictionary;

internal static class LoadDictionary
{
  public static async Task<Dictionary<string, string>> LoadFromLocalCacheAsync(string filePath)
  {
    byte[] json = await File.ReadAllBytesAsync(filePath);
    return JsonSerializer.Deserialize<Dictionary<string, string>>(json, Dictionary.Options);
  }
}
