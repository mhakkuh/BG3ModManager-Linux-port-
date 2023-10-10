using PowerArgs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Args
{
	public class ScriptExtenderUpdaterArgs
	{
		[ArgShortcut("-u"), ArgDescription("The path to DWrite.dll"), ArgRequired]
		public string? Updater { get; set; }

		[ArgShortcut("-g"), ArgDescription("The path to bg3.exe"), ArgRequired]
		public string? Game { get; set; }

		[ArgShortcut("-c"), ArgDescription("The path to ScriptExtenderUpdaterConfig.json")]
		public string? Config { get; set; }
	}
}
