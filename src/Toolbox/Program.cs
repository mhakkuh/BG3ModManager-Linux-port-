
using Toolbox.ScriptExtender;

using (var updater = new Updater("C:\\BG3\\bin\\DWrite.dll", "C:\\BG3\\bin\\ScriptExtenderUpdaterConfig.json"))
{
	//updater.ShowConsoleWindow();
	updater.SetGameVersion("C:\\BG3\\bin\\bg3.exe");
	updater.Update();
	Console.WriteLine("Done.");
}
Console.WriteLine("Press any key to close.");
Console.ReadKey();