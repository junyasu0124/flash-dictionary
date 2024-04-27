using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
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
  /// <inheritdoc cref="TranslateAsync(string, string?, string, CancellationToken)"/>
  public static async Task<string?> TranslateAsync(string sentence)
  {
    return await TranslateAsync(sentence, null, "ja", CancellationToken.None);
  }

  /// <inheritdoc cref="TranslateAsync(string)"/>/>
  /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
  public static async Task<string?> TranslateAsync(string sentence, CancellationToken cancellationToken)
  {
    return await TranslateAsync(sentence, null, "ja", cancellationToken);
  }

  /// <inheritdoc cref="TranslateAsync(string, string?, string, CancellationToken)"/>
  public static async Task<string?> TranslateAsync(string sentence, string? sourceLanguage, string targetLanguage)
  {
    return await TranslateAsync(sentence, sourceLanguage, targetLanguage, CancellationToken.None);
  }

  /// <summary>
  /// Get translation from Google Translation.
  /// </summary>
  /// <param name="sentence">Sentence to translate</param>
  /// <param name="sourceLanguage">Language of the text to translate. <see langword="null"/> for auto-detect.</param>
  /// <param name="targetLanguage">Language to translate to.</param>
  /// <returns></returns>
  public static async Task<string?> TranslateAsync(string sentence, string? sourceLanguage, string targetLanguage, CancellationToken cancellationToken)
  {
    var data = new Dictionary<string, string>
    {
      { "text", sentence },
      { "source", sourceLanguage ?? "" },
      { "target", targetLanguage }
    };
    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
      var response = await client.PostAsync(url, content, cancellationToken);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        return null;
      return await response.Content.ReadAsStringAsync(cancellationToken);
    }
    catch
    {
      return null;
    }
  }
}
