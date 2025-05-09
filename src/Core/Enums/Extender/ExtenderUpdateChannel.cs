using Newtonsoft.Json.Converters;

using System.ComponentModel;

namespace DivinityModManager.Enums.Extender;

[JsonConverter(typeof(StringEnumConverter))]
public enum ExtenderUpdateChannel
{
	[Description("Release")]
	Release,
	[Description("Devel")]
	Devel,
	[Description("Nightly")]
	Nightly
}
