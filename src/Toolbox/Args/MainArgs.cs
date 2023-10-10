using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PowerArgs;

using Toolbox.ScriptExtender;

namespace Toolbox.Args
{
	[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
	public class MainArgs
	{
		[ArgActionMethod, ArgDescription("Updates the script extender using the updater dll")]
		public void UpdateScriptExtender(ScriptExtenderUpdaterArgs args)
		{
			if(!File.Exists(args.Updater) || !File.Exists(args.Game))
			{
				throw new FileNotFoundException("-updater and -game must be valid file paths.");
			}
			using (var updater = new Updater(args.Updater, args.Config))
			{
				//updater.ShowConsoleWindow();
				updater.SetGameVersion(args.Game);
				Console.WriteLine($"Updating...");
				updater.Update();
			}
			Console.WriteLine("Update finished");
		}
	}
}
