using DivinityModManager.Models;
using DivinityModManager.Models.Cache;
using DivinityModManager.Util;

using Newtonsoft.Json;

namespace DivinityModManager.ModUpdater.Cache
{
	public class SteamWorkshopCacheHandler : IExternalModCacheHandler<SteamWorkshopCachedData>
	{
		public ModSourceType SourceType => ModSourceType.STEAM;
		public string FileName => "workshopdata.json";
		public JsonSerializerSettings SerializerSettings => ModUpdateHandler.DefaultSerializerSettings;
		public SteamWorkshopCachedData CacheData { get; set; }
		public bool IsEnabled { get; set; } = false;

		public string SteamAppID { get; set; }

		public SteamWorkshopCacheHandler() : base()
		{
			CacheData = new SteamWorkshopCachedData();
		}

		public async Task<bool> Update(IEnumerable<DivinityModData> mods, CancellationToken cts)
		{
			var success = await WorkshopDataLoader.GetAllWorkshopDataAsync(CacheData, SteamAppID, cts);
			if (success)
			{
				var cachedGUIDs = CacheData.Mods.Keys.ToHashSet();
				var nonWorkshopMods = mods.Where(x => !cachedGUIDs.Contains(x.UUID)).ToList();
				if (nonWorkshopMods.Count > 0)
				{
					foreach (var m in nonWorkshopMods)
					{
						CacheData.AddNonWorkshopMod(m.UUID);
					}
				}
			}
			return false;
		}
	}
}
