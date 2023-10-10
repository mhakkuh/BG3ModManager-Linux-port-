using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Updates
{
	public class UpdateResult
	{
		public List<DivinityModData> UpdatedMods { get; set; }
		public string FailureMessage { get; set; }
		public bool Success { get; set; }

		public UpdateResult()
		{
			UpdatedMods = new List<DivinityModData>();
			Success = true;
		}
	}
}
