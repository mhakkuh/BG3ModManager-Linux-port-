using CrossSpeak;

namespace DivinityModManager.Util.ScreenReader;

public static partial class ScreenReaderHelper
{
	private static readonly string[] _dlls = ["nvdaControllerClient64.dll", "SAAPI64.dll", "Tolk.dll"];
	private static bool _loadedDlls = false;

	public static void Close()
	{
		if(CrossSpeakManager.Instance.IsLoaded())
		{
			CrossSpeakManager.Instance.Close();
		}
	}

	private static bool EnsureInit()
	{
		if(!_loadedDlls)
		{
			var libPath = Path.Combine(DivinityApp.GetAppDirectory(), "_Lib");
			foreach (var dll in _dlls)
			{
				var filePath = Path.Combine(libPath, dll);
				try
				{
					if (File.Exists(filePath))
					{
						NativeLibraryHelper.LoadLibrary(filePath);
					}
				}
				catch(Exception ex)
				{
					DivinityApp.Log($"Error loading '{dll}':\n{ex}");
				}
			}
			_loadedDlls = true;
		}
		if (!CrossSpeakManager.Instance.IsLoaded())
		{
			CrossSpeakManager.Instance.TrySAPI(true);
			CrossSpeakManager.Instance.Initialize();
		}
		return CrossSpeakManager.Instance.IsLoaded();
	}

	public static void Output(string text, bool interrupt = true)
	{
		if(EnsureInit())
		{
			CrossSpeakManager.Instance.Output(text, interrupt);
		}
	}

	public static void Speak(string text, bool interrupt = true)
	{
		if (EnsureInit())
		{
			CrossSpeakManager.Instance.Output(text, interrupt);
		}
	}
}
