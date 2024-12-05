namespace DivinityModManager.Models;

public class WindowSettings
{
	public bool Maximized { get; set; }
	public int Screen { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }

	public WindowSettings()
	{
		Maximized = false;
		X = 0;
		Y = 0;
		Width = -1;
		Height = -1;
		Screen = -1;
	}
}
