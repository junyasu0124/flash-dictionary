using System;
using System.IO;
using System.Security.Cryptography;
using Windows.Storage;

namespace FlashDictionary.Util;

internal static class Hash
{
  public static string CalculateFileHash(string filePath)
  {
    using var md5 = MD5.Create();
    using var stream = File.OpenRead(filePath);
    var hash = md5.ComputeHash(stream);
    var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    return hashString;
  }
  public static string CalculateFileHash(StorageFile file)
  {
    return CalculateFileHash(file.Path);
  }
}
