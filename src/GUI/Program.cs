using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DivinityModManager
{
	internal class Program
	{
		private static SplashScreen _splash;

		[STAThread]
		static void Main(string[] args)
		{
			_splash = new SplashScreen("Resources/BG3MMSplashScreen.png");
			_splash.Show(false, false);

			var app = new App
			{
				Splash = _splash
			};
			app.InitializeComponent();
			app.Run();
		}
	}
}
