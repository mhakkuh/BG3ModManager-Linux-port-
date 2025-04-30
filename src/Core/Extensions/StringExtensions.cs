namespace DivinityModManager.Extensions;

public static class StringExtensions
{
	public static bool IsExistingDirectory(this string path)
	{
		return !String.IsNullOrWhiteSpace(path) && Directory.Exists(path);
	}

	public static bool IsExistingFile(this string path)
	{
		return !String.IsNullOrWhiteSpace(path) && File.Exists(path);
	}

	/*
	 * MaybeAddReplacement("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
		MaybeAddReplacement("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		MaybeAddReplacement("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
	 * */

	private static Dictionary<string, string> _specialPaths = new Dictionary<string, string>()
	{
		{ "%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) },
		{ "%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) },
		{ "%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
	};

	public static string ReplaceSpecialPaths(this string path)
	{
		if (string.IsNullOrEmpty(path)) return path;

		foreach (var kvp in _specialPaths)
		{
			if(!string.IsNullOrEmpty(kvp.Value))
			{
				path = path.Replace(kvp.Value, kvp.Key);
			}
		}
		return path;
	}
}
