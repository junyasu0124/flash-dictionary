using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace FlashDictionary.Core.Dictionary;
internal static class Dictionary
{
  public static JsonSerializerOptions Options { get; } = new()
  {
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
  };
}
