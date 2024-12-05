

using DivinityModManager.AppServices;
using DivinityModManager.Util;
using DivinityModManager.ViewModels;
using DivinityModManager.Views;

using Onova;
using Onova.Services;

using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace DivinityModManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public SplashScreen Splash { get; set; }

	public App()
	{
		Services.RegisterSingleton<IFileWatcherService>(new FileWatcherService());

		var client = new HttpClient();
		client.DefaultRequestHeaders.Add("User-Agent", AppDomain.CurrentDomain.FriendlyName);
		Services.RegisterSingleton(client);

		var updateManager = new UpdateManager(
#if DEBUG
		new LocalPackageResolver(DivinityApp.GetAppDirectory("TestUpdates"), "*.zip"),
#else
		new GithubPackageResolver(client, DivinityApp.GITHUB_USER, DivinityApp.GITHUB_REPO, DivinityApp.GITHUB_RELEASE_ASSET),
#endif
		new ZipPackageExtractor());
		Services.RegisterSingleton(updateManager);

		Services.RegisterSingleton(new AppUpdateWindowViewModel(updateManager));

		// POCO type warning suppression
		Services.Register<ICreatesObservableForProperty>(() => new DivinityModManager.Util.CustomPropertyResolver());
#if DEBUG
		RxApp.SuppressViewCommandBindingMessage = false;
#else
		RxApp.DefaultExceptionHandler = new RxExceptionHandler();
		RxApp.SuppressViewCommandBindingMessage = true;
#endif
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		//For making date display use the current system's culture
		FrameworkElement.LanguageProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

		EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));

		var splashFade = new System.Threading.Thread(() =>
		{
			Splash.Close(TimeSpan.FromSeconds(1));
		});

		var mainWindow = new MainWindow();
		splashFade.Start();
		mainWindow.Show();
	}

	private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		DivinityApp.IsKeyboardNavigating = false;
	}
}
