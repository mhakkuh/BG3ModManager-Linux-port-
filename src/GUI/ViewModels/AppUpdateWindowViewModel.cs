using DivinityModManager.Util;
using DivinityModManager.Views;

using Onova;

using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DivinityModManager.ViewModels;
public partial class AppUpdateWindowViewModel : ReactiveObject
{
	private readonly UpdateManager _updateManager;

	[Reactive] public bool IsVisible { get; set; }
	[Reactive] public bool CanConfirm { get; set; }
	[Reactive] public bool CanSkip { get; set; }
	[Reactive] public string AppTitle { get; set; }
	[Reactive] public Version AppVersion { get; set; }
	[Reactive] public string SkipButtonText { get; set; }
	[Reactive] public string UpdateDescription { get; set; }
	[Reactive] public string UpdateChangelogView { get; set; }
	[Reactive] public Version UpdateVersion { get; set; }

	public ICommand ConfirmCommand { get; private set; }
	public ICommand SkipCommand { get; private set; }

	[GeneratedRegex(@"^\s+$[\r\n]*", RegexOptions.Multiline)]
	private static partial Regex RemoveEmptyLinesRe();

	private static readonly Regex RemoveEmptyLinesPattern = RemoveEmptyLinesRe();

	public async Task RunUpdateAsync(CancellationToken token)
	{
		try
		{
			MainWindow.Self.ViewModel.Settings.LastUpdateCheck = DateTimeOffset.Now.ToUnixTimeSeconds();
			MainWindow.Self.ViewModel.SaveSettings();
			await _updateManager.PrepareUpdateAsync(UpdateVersion, null, token);
			_updateManager.LaunchUpdater(UpdateVersion, true);
			Environment.Exit(0);
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error updating program:\n{ex}");
			IsVisible = false;
		}
	}

	private bool _showAlert;

	public async Task CheckForUpdatesAsync(IScheduler scheduler, CancellationToken token)
	{
		try
		{
			var markdownText = await WebHelper.DownloadUrlAsStringAsync(DivinityApp.URL_CHANGELOG_RAW, CancellationToken.None);
			if (!String.IsNullOrEmpty(markdownText))
			{
				markdownText = RemoveEmptyLinesPattern.Replace(markdownText, string.Empty);
				await Observable.Start(() =>
				{
					UpdateChangelogView = markdownText;
				}, RxApp.MainThreadScheduler);
			}

			var result = await _updateManager.CheckForUpdatesAsync(token);
			if (result.CanUpdate)
			{
				UpdateDescription = $"{AppTitle} {result.LastVersion} is now available.\nYou have version {AppVersion} installed.";

				CanConfirm = true;
				SkipButtonText = "Skip";
				CanSkip = true;
				UpdateVersion = result.LastVersion;
				if (_showAlert) MainWindow.Self.ViewModel.ShowAlert("Update found!", AlertType.Success, 30);
			}
			else
			{
				UpdateDescription = $"{AppTitle} is up-to-date.\nYou have version {AppVersion} installed.";
				CanConfirm = false;
				CanSkip = true;
				SkipButtonText = "Close";
				if (_showAlert) MainWindow.Self.ViewModel.ShowAlert("Already up-to-date", AlertType.Info, 30);
			}

			if (result.CanUpdate || _showAlert)
			{
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					IsVisible = true;
				});
			}
		}
		catch(Exception ex)
		{
			DivinityApp.Log($"Error checking for update:\n{ex}");
			if (_showAlert) MainWindow.Self.ViewModel.ShowAlert($"Error occurred when checking for updates: {ex.Message}", AlertType.Danger, 60);

			if (ex is System.Net.WebException)
			{
				MainWindow.Self.DisplayError("Update Check Failed", "There was a problem reaching the update server. Please check your internet connection and try again later.", false);
			}
		}
	}

	private IDisposable _updateTask = null;

	public void ScheduleUpdateCheck(bool showAlerts = false)
	{
		_showAlert = showAlerts;
		_updateTask?.Dispose();
		_updateTask = RxApp.TaskpoolScheduler.ScheduleAsync(CheckForUpdatesAsync);
	}

	public AppUpdateWindowViewModel(UpdateManager updateManager)
	{
		_updateManager = updateManager;
		var canConfirm = this.WhenAnyValue(x => x.CanConfirm);
		ConfirmCommand = ReactiveCommand.Create(() =>
		{
			RxApp.MainThreadScheduler.ScheduleAsync(async (sch, t) =>
			{
				await RunUpdateAsync(t);
			});
		}, canConfirm, RxApp.MainThreadScheduler);

		var canSkip = this.WhenAnyValue(x => x.CanSkip);
		SkipCommand = ReactiveCommand.Create(() => IsVisible = false, canSkip);
		CanSkip = true;
	}
}