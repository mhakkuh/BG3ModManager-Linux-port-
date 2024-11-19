

using DivinityModManager.AppServices;
using DivinityModManager.Util;
using DivinityModManager.Views;

using ReactiveUI;

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace DivinityModManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public SplashScreen Splash { get; set; }

		public App()
		{
			Services.RegisterSingleton<IFileWatcherService>(new FileWatcherService());

			// POCO type warning suppression
			Services.Register<ICreatesObservableForProperty>(() => new DivinityModManager.Util.CustomPropertyResolver());

			WebHelper.SetupClient();
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
}
