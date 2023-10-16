using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DivinityModManager.Util
{
	public static class PropertyConverters
	{
		public static Visibility BoolToVisibility(bool b) => b ? Visibility.Visible : Visibility.Collapsed;
	}
}
