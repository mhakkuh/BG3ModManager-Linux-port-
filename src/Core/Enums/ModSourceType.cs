using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager
{
	public enum ModSourceType
	{
		[Description("None")]
		NONE,
		[Description("Steam Workshop")]
		STEAM,
		[Description("Nexus Mods")]
		NEXUSMODS,
		[Description("Github")]
		GITHUB
	}
}
