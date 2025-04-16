using CrossSpeak;

namespace DivinityModManager;

public static class ScreenReaderHelper
{
	public static void Close()
	{
		if(CrossSpeakManager.Instance.IsLoaded())
		{
			CrossSpeakManager.Instance.Close();
		}
	}

	private static bool EnsureInit()
	{
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
