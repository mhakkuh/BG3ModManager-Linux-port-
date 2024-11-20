using AutoUpdaterDotNET;

using DivinityModManager.Controls;
using DivinityModManager.Util;

using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace DivinityModManager.Views;

public class AppUpdateWindowBase : HideWindowBase<AppUpdateWindowViewModel> { }

public partial class AppUpdateWindowViewModel : ReactiveObject
{
	private readonly AppUpdateWindow _view;

	public UpdateInfoEventArgs UpdateArgs { get; set; }

	[Reactive] public bool IsVisible { get; set; }
	[Reactive] public bool CanConfirm { get; set; }
	[Reactive] public bool CanSkip { get; set; }
	[Reactive] public string SkipButtonText { get; set; }
	[Reactive] public string UpdateDescription { get; set; }
	[Reactive] public string UpdateChangelogView { get; set; }

	public ICommand ConfirmCommand { get; private set; }
	public ICommand SkipCommand { get; private set; }

	[GeneratedRegex(@"^\s+$[\r\n]*", RegexOptions.Multiline)]
	private static partial Regex RemoveEmptyLinesRe();

	private static readonly Regex RemoveEmptyLinesPattern = RemoveEmptyLinesRe();

	private async Task CheckArgsAsync(IScheduler scheduler, CancellationToken token)
	{
		var markdownText = await WebHelper.DownloadUrlAsStringAsync(DivinityApp.URL_CHANGELOG_RAW, CancellationToken.None);

		RxApp.MainThreadScheduler.Schedule(() =>
		{
			if (!String.IsNullOrEmpty(markdownText))
			{
				markdownText = RemoveEmptyLinesPattern.Replace(markdownText, string.Empty);
				UpdateChangelogView = markdownText;
			}

			if (UpdateArgs.IsUpdateAvailable)
			{
				UpdateDescription = $"{AutoUpdater.AppTitle} {UpdateArgs.CurrentVersion} is now available.\nYou have version {UpdateArgs.InstalledVersion} installed.";

				CanConfirm = true;
				SkipButtonText = "Skip";
				CanSkip = UpdateArgs.Mandatory?.Value != true;
			}
			else
			{
				UpdateDescription = $"{AutoUpdater.AppTitle} is up-to-date.";
				CanConfirm = false;
				CanSkip = true;
				SkipButtonText = "Close";
			}

			IsVisible = true;
		});
	}

	public AppUpdateWindowViewModel(AppUpdateWindow view)
	{
		_view = view;

		var canConfirm = this.WhenAnyValue(x => x.CanConfirm);
		ConfirmCommand = ReactiveCommand.Create(() =>
		{
			try
			{
				if (AutoUpdater.DownloadUpdate(UpdateArgs))
				{
					System.Windows.Application.Current.Shutdown();
				}
			}
			catch (Exception ex)
			{
				MainWindow.Self.DisplayError($"Error occurred while updating:\n{ex}");
				_view.Hide();
			}
		}, canConfirm, RxApp.MainThreadScheduler);

		var canSkip = this.WhenAnyValue(x => x.CanSkip);
		SkipCommand = ReactiveCommand.Create(() => _view.Hide(), canSkip);
	}

	public void CheckArgsAndOpen(UpdateInfoEventArgs args)
	{
		UpdateArgs = args;
		RxApp.TaskpoolScheduler.ScheduleAsync(CheckArgsAsync);
	}
}

public partial class AppUpdateWindow : AppUpdateWindowBase
{
	private readonly Lazy<Markdown> _fallbackMarkdown = new(() => new Markdown());
	private readonly Markdown _defaultMarkdown;

	private FlowDocument StringToMarkdown(string text)
	{
		var markdown = _defaultMarkdown ?? _fallbackMarkdown.Value;
		var doc = markdown.Transform(text);
		return doc;
	}

	public override void HideWindow_Closing(object sender, CancelEventArgs e)
	{
		base.HideWindow_Closing(sender, e);
		ViewModel.IsVisible = false;
	}

	public AppUpdateWindow()
	{
		InitializeComponent();

		ViewModel = new AppUpdateWindowViewModel(this);

		var obj = TryFindResource("DefaultMarkdown");
		if (obj != null && obj is Markdown markdown)
		{
			_defaultMarkdown = markdown;
		}

		this.WhenActivated(d =>
		{
			d(this.BindCommand(ViewModel, vm => vm.ConfirmCommand, v => v.ConfirmButton));
			d(this.BindCommand(ViewModel, vm => vm.SkipCommand, v => v.SkipButton));
			d(this.OneWayBind(ViewModel, vm => vm.SkipButtonText, v => v.SkipButton.Content));
			d(this.OneWayBind(ViewModel, vm => vm.UpdateDescription, v => v.UpdateDescription.Text));
			d(this.OneWayBind(ViewModel, vm => vm.UpdateChangelogView, v => v.UpdateChangelogView.Document, StringToMarkdown));
		});
	}
}
