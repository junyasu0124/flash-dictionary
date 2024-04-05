using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashDictionary.Core.Translation;

internal static class TranslatedByGoogle
{
  /// <summary>
  /// URL of Google Apps Script for translation.
  /// </summary>
  private readonly static string url = "https://script.google.com/macros/s/AKfycbw5rGTNx0tp9xn57ckqRa_rFnmXbe9PX_uzn0drhsbKFbVXVqm5nkwSaIDLWlXT5Ybu/exec";
  private readonly static HttpClient client = new();

  /// <summary>
  /// Get translation into Japanese from Google Translation.
  /// </summary>
  /// <param name="text"></param>
  /// <returns></returns>
  public static async Task<string?> TranslateAsync(string text)
  {
    return await TranslateAsync(text, null, "ja");
  }

  /// <summary>
  /// Get translation from Google Translation.
  /// </summary>
  /// <param name="text">Text to translate</param>
  /// <param name="sourceLanguage">Language of the text to translate. <see langword="null"/> for auto-detect.</param>
  /// <param name="targetLanguage">Language to translate to.</param>
  /// <returns></returns>
  public static async Task<string?> TranslateAsync(string text, string? sourceLanguage, string targetLanguage)
  {
    var data = new Dictionary<string, string>
    {
      { "text", text },
      { "source", sourceLanguage ?? "" },
      { "target", targetLanguage }
    };
    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
      var response = await client.PostAsync(url, content);
      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        return null;
      return await response.Content.ReadAsStringAsync();
    }
    catch (Exception)
    {
      return null;
    }
  }
}
