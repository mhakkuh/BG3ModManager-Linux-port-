using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models.Github
{
	public class GithubModData : ReactiveObject
	{
		[Reactive] public string Author { get; set; }
		[Reactive] public string Repository { get; set; }
		[Reactive] public Uri LatestRelease { get; set; }

		public void Update(GithubModData data)
		{
			//TODO
			Author = data.Author;
			Repository = data.Repository;
			LatestRelease = data.LatestRelease;
		}
	}
}
