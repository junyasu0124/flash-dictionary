using Microsoft.UI.Xaml;

namespace FlashDictionary;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
  public static MainWindow Window { get; set; }

  /// <summary>
  /// Initializes the singleton application object.  This is the first line of authored code
  /// executed, and as such is the logical equivalent of main() or WinMain().
  /// </summary>
  public App()
  {
    InitializeComponent();
  }

  /// <summary>
  /// Invoked when the application is launched.
  /// </summary>
  /// <param name="args">Details about the launch request and process.</param>
  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    Window = new MainWindow();
    Window.Activate();
  }
}
