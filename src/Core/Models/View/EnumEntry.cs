using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.View
{
	public class EnumEntry : ReactiveObject
	{
		[Reactive] public string Description { get; set; }
		[Reactive] public object Value { get; set; }

		public EnumEntry(string description, object value)
		{
			Description = description;
			Value = value;
		}
	}
}
