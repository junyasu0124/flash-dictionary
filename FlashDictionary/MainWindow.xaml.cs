using FlashDictionary.Core.Dictionary;
using FlashDictionary.Core.Translation;
using FlashDictionary.Core.Translation.InDictionaries.Normalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using WinRT.Interop;

namespace FlashDictionary;

public sealed partial class MainWindow : Window
{
  public IntPtr HWnd => WindowNative.GetWindowHandle(this);

  public MainWindow()
  {
    InitializeComponent();

    var appWindow = GetAppWindowForCurrentWindow();

    appWindow.Title = "Flash Dictionary";
    // appWindow.SetIcon("");
  }

  private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
  {
    var wndId = Win32Interop.GetWindowIdFromWindow(HWnd);
    return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
  }

  private async void Parent_Panel_Loaded(object sender, RoutedEventArgs e)
  {
    //if (await IsEdited.Check())

    //{
    //  var loadDictionaryFileStart = DateTime.Now;
    //  var loadResult = await LoadDictionaryFile.LoadAsync("""C:\Users\yasue\Downloads\ejdict\ejdict-hand-utf8.txt""", DictionaryDataFormat.TabSeparated, Encoding.UTF8, true, true);
    //  Result_Header.Text = $"LoadDictionaryFile: {(DateTime.Now - loadDictionaryFileStart).TotalMilliseconds} ms, {Enum.GetName(typeof(LoadState), loadResult)}";
    //}

    //{
    //  var loadDictionaryFileStart = DateTime.Now;
    //  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    //  var loadResult = await LoadDictionaryFile.LoadAsync("""C:\Users\yasue\Downloads\EIJIRO144-10\EIJIRO144-10.txt""", DictionaryDataFormat.Eijiro, Encoding.GetEncoding("shift_jis"), true, true);
    //  Result_Header.Text = $"LoadDictionaryFile: {(DateTime.Now - loadDictionaryFileStart).TotalMilliseconds} ms, {Enum.GetName(typeof(LoadState), loadResult)}";
    //  if (loadResult == LoadState.UnknownErrored)
    //  {
    //    Result_Header.Text += LoadDictionaryFile.exception.Message;
    //  }
    //}

    {
      var loadPositionsStart = DateTime.Now;
      await LoadPositions.LoadAsync();
      Result_Header.Text = $"LoadPositions: {(DateTime.Now - loadPositionsStart).TotalMilliseconds} ms";
    }
    await Base.LoadData();
  }

  private readonly int translationDebounceMilliseconds = 500;
#pragma warning disable CS0649
  private readonly CancellationTokenSource? translationDebounceTokenSource;
#pragma warning restore CS0649
  private void Input_Box_TextChanged(object sender, TextChangedEventArgs e)
  {
    Debounce(Translate, translationDebounceMilliseconds, translationDebounceTokenSource);
  }
  private void Input_Confirm_Click(object sender, RoutedEventArgs e)
  {
    Debounce(Translate, translationDebounceMilliseconds, translationDebounceTokenSource);
  }

  private CancellationTokenSource? translatedByGoogleTokenSource;
  private void Translate()
  {
    DispatcherQueue.TryEnqueue(async () =>
    {
      var text = Input_Box.Text;
      if (!string.IsNullOrWhiteSpace(text))
      {
        var start = DateTime.Now;
        var inDictionaries = TranslateInDictionaries.Translate(text, out var searchedWordCount);
        Result_Header.Text = $"InDictionaries: {(DateTime.Now - start).TotalMilliseconds} ms, {searchedWordCount}";
        if (inDictionaries is not null)
        {
          Result_InDictionaries.ItemsSource = inDictionaries.Items;
        }

        translatedByGoogleTokenSource?.Cancel();
        translatedByGoogleTokenSource = new();
        var byGoogle = await TranslatedByGoogle.TranslateAsync(text, translatedByGoogleTokenSource.Token);
        if (byGoogle is not null)
        {
          Result_ByGoogle.Text = byGoogle;
        }
      }
    });
  }

  private static void Debounce(Action action, int milliseconds, CancellationTokenSource? tokenSource)
  {
    if (tokenSource == null || tokenSource.IsCancellationRequested)
    {
      tokenSource = new();

      action();
    }
    else
    {
      tokenSource?.Cancel();
      tokenSource = new();

      Task.Delay(milliseconds, tokenSource.Token).ContinueWith(task =>
      {
        if (task.IsCompletedSuccessfully)
        {
          action();
        }
      }, TaskScheduler.Default);
    }
  }
}
