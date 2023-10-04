using AdonisUI;

using Alphaleonis.Win32.Filesystem;

using DivinityModManager.Converters;
using DivinityModManager.Models.App;
using DivinityModManager.Util;
using DivinityModManager.Util.ScreenReader;
using DivinityModManager.ViewModels;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DivinityModManager.Views
{
	public class MainViewControlViewBase : ReactiveUserControl<MainWindowViewModel> { }

	public partial class MainViewControl : MainViewControlViewBase
	{
		private readonly MainWindow main;

		public bool UserInvokedUpdate { get; set; }

		private readonly Dictionary<string, MenuItem> menuItems = new Dictionary<string, MenuItem>();
		public Dictionary<string, MenuItem> MenuItems => menuItems;

		public HorizontalModLayout GetModLayout()
		{
			return MainContentPresenter.FindVisualChildren<HorizontalModLayout>().FirstOrDefault();
		}

		private static System.Windows.Shell.TaskbarItemProgressState BoolToTaskbarItemProgressState(bool b)
		{
			return b ? System.Windows.Shell.TaskbarItemProgressState.Normal : System.Windows.Shell.TaskbarItemProgressState.None;
		}

		private void RegisterBindings()
		{
			this.OneWayBind(ViewModel, vm => vm.HideModList, view => view.ModListRectangle.Visibility, BoolToVisibilityConverter.FromBool);
			this.OneWayBind(ViewModel, vm => vm.MainProgressIsActive, view => view.MainBusyIndicator.IsBusy);

			foreach (var key in ViewModel.Keys.All)
			{
				var keyBinding = new KeyBinding(key.Command, key.Key, key.Modifiers);
				BindingOperations.SetBinding(keyBinding, InputBinding.CommandProperty, new Binding { Path = new PropertyPath("Command"), Source = key });
				BindingOperations.SetBinding(keyBinding, KeyBinding.KeyProperty, new Binding { Path = new PropertyPath("Key"), Source = key });
				BindingOperations.SetBinding(keyBinding, KeyBinding.ModifiersProperty, new Binding { Path = new PropertyPath("Modifiers"), Source = key });
				main.InputBindings.Add(keyBinding);
			}

			//Initial keyboard focus by hitting up or down
			var setInitialFocusCommand = ReactiveCommand.Create(() =>
			{
				if (!DivinityApp.IsKeyboardNavigating && this.ViewModel.ActiveSelected == 0 && this.ViewModel.InactiveSelected == 0)
				{
					GetModLayout()?.FocusInitialActiveSelected();
				}
			});
			main.InputBindings.Add(new KeyBinding(setInitialFocusCommand, Key.Up, ModifierKeys.None));
			main.InputBindings.Add(new KeyBinding(setInitialFocusCommand, Key.Down, ModifierKeys.None));

			foreach (var item in TopMenuBar.Items)
			{
				if (item is MenuItem entry)
				{
					if (entry.Header is string label)
					{
						menuItems.Add(label, entry);
					}
					else if (!String.IsNullOrWhiteSpace(entry.Name))
					{
						menuItems.Add(entry.Name, entry);
					}
				}
			}

			//Generating menu items
			var menuKeyProperties = typeof(AppKeys)
			.GetRuntimeProperties()
			.Where(prop => Attribute.IsDefined(prop, typeof(MenuSettingsAttribute)))
			.Select(prop => typeof(AppKeys).GetProperty(prop.Name));
			foreach (var prop in menuKeyProperties)
			{
				Hotkey key = (Hotkey)prop.GetValue(ViewModel.Keys);
				MenuSettingsAttribute menuSettings = prop.GetCustomAttribute<MenuSettingsAttribute>();
				if (String.IsNullOrEmpty(key.DisplayName))
					key.DisplayName = menuSettings.DisplayName;

				if (!menuItems.TryGetValue(menuSettings.Parent, out MenuItem parentMenuItem))
				{
					parentMenuItem = new MenuItem
					{
						Header = menuSettings.Parent
					};
					TopMenuBar.Items.Add(parentMenuItem);
					menuItems.Add(menuSettings.Parent, parentMenuItem);
				}

				MenuItem newEntry = new MenuItem
				{
					Header = menuSettings.DisplayName,
					InputGestureText = key.ToString(),
					Command = key.Command
				};
				BindingOperations.SetBinding(newEntry, MenuItem.CommandProperty, new Binding { Path = new PropertyPath("Command"), Source = key });
				parentMenuItem.Items.Add(newEntry);
				if (!String.IsNullOrWhiteSpace(menuSettings.Tooltip))
				{
					newEntry.ToolTip = menuSettings.Tooltip;
				}
				if (!String.IsNullOrWhiteSpace(menuSettings.Style))
				{
					Style style = (Style)TryFindResource(menuSettings.Style);
					if (style != null)
					{
						newEntry.Style = style;
					}
				}

				if (menuSettings.AddSeparator)
				{
					parentMenuItem.Items.Add(new Separator());
				}

				menuItems.Add(prop.Name, newEntry);
			}
		}

		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CachedAutomationPeer(this);
		}

		public void UpdateColorTheme(bool darkMode)
		{
			ResourceLocator.SetColorScheme(this.Resources, !darkMode ? DivinityApp.LightTheme : DivinityApp.DarkTheme);
			main.UpdateColorTheme(darkMode);
		}

		private void AlertBar_Show(object sender, RoutedEventArgs e)
		{
			var spStandard = (StackPanel)AlertBar.FindName("spStandard");
			var spOutline = (StackPanel)AlertBar.FindName("spOutline");

			Grid grdParent;
			switch (AlertBar.Theme)
			{
				case DivinityModManager.Controls.AlertBar.ThemeType.Standard:
					grdParent = spStandard.FindVisualChildren<Grid>().FirstOrDefault();
					break;
				case DivinityModManager.Controls.AlertBar.ThemeType.Outline:
				default:
					grdParent = spOutline.FindVisualChildren<Grid>().FirstOrDefault();
					break;
			}

			TextBlock lblMessage = grdParent.FindVisualChildren<TextBlock>().FirstOrDefault();
			if (lblMessage != null)
			{
				DivinityApp.Log(lblMessage.Text);
			}
		}

		private void ComboBox_KeyDown_LoseFocus(object sender, KeyEventArgs e)
		{
			bool loseFocus = false;
			if ((e.Key == Key.Enter || e.Key == Key.Return))
			{
				UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
				elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
				ViewModel.StopRenaming(false);
				loseFocus = true;
				e.Handled = true;
			}
			else if (e.Key == Key.Escape)
			{
				ViewModel.StopRenaming(true);
				loseFocus = true;
			}

			if (loseFocus && sender is ComboBox comboBox)
			{
				var tb = comboBox.FindVisualChildren<TextBox>().FirstOrDefault();
				tb?.Select(0, 0);
			}
		}

		private void OrdersComboBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender is ComboBox comboBox && ViewModel.IsRenamingOrder)
			{
				RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(250), _ =>
				{
					var tb = comboBox.FindVisualChildren<TextBox>().FirstOrDefault();
					if (tb != null && !tb.IsFocused)
					{
						var cancel = String.IsNullOrEmpty(tb.Text);
						ViewModel.StopRenaming(cancel);
						if (!cancel)
						{
							ViewModel.SelectedModOrder.Name = tb.Text;
							var directory = Path.GetDirectoryName(ViewModel.SelectedModOrder.FilePath);
							var ext = Path.GetExtension(ViewModel.SelectedModOrder.FilePath);
							string outputName = DivinityModDataLoader.MakeSafeFilename(Path.Combine(ViewModel.SelectedModOrder.Name + ext), '_');
							ViewModel.SelectedModOrder.FilePath = Path.Combine(directory, outputName);
							AlertBar.SetSuccessAlert($"Renamed load order name/path to '{ViewModel.SelectedModOrder.FilePath}'", 20);
						}
					}
				});
			}
		}

		private void OrderComboBox_OnUserClick(object sender, MouseButtonEventArgs e)
		{
			RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
			{
				if (ViewModel.Settings != null && ViewModel.Settings.LastOrder != ViewModel.SelectedModOrder.Name)
				{
					ViewModel.Settings.LastOrder = ViewModel.SelectedModOrder.Name;
					ViewModel.SaveSettings();
				}
			});
		}

		private void OrdersComboBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is ComboBox ordersComboBox)
			{
				var tb = ordersComboBox.FindVisualChildren<TextBox>().FirstOrDefault();
				if (tb != null)
				{
					tb.ContextMenu = ordersComboBox.ContextMenu;
					tb.ContextMenu.DataContext = ViewModel;
				}
			}
		}

		private readonly Dictionary<string, string> _shortcutButtonBindings = new Dictionary<string, string>()
		{
			["OpenWorkshopFolderButton"] = "Keys.OpenWorkshopFolder.Command",
			["OpenModsFolderButton"] = "Keys.OpenModsFolder.Command",
			["OpenExtenderLogsFolderButton"] = "Keys.OpenLogsFolder.Command",
			["OpenGameButton"] = "Keys.LaunchGame.Command",
			["LoadGameMasterModOrderButton"] = "Keys.ImportOrderFromSelectedGMCampaign.Command",
		};

		private void ModOrderPanel_Loaded(object sender, RoutedEventArgs e)
		{
			//var orderPanel = (Grid)this.FindResource("ModOrderPanel");
			if (sender is Grid orderPanel)
			{
				var buttons = orderPanel.FindVisualChildren<Button>();
				foreach (var button in buttons)
				{
					if (_shortcutButtonBindings.TryGetValue(button.Name, out string path))
					{
						if (button.Command == null)
						{
							BindingHelper.CreateCommandBinding(button, path, ViewModel);
						}
					}
				}
			};
		}

		private void GameMasterCampaignComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel.UserChangedSelectedGMCampaign = true;
		}

		public void OnActivated()
		{
			this.WhenAnyValue(x => x.ViewModel.MainProgressIsActive).Take(1).Delay(TimeSpan.FromMilliseconds(25)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(b =>
			{
				this.MainBusyIndicator.Visibility = Visibility.Visible;
			});
			RegisterBindings();

			this.DeleteFilesView.ViewModel.FileDeletionComplete += (o, e) =>
			{
				DivinityApp.Log($"Deleted {e.TotalFilesDeleted} file(s).");
				if (e.TotalFilesDeleted > 0)
				{
					var deletedUUIDs = e.DeletedFiles.Where(x => !x.IsWorkshop).Select(x => x.UUID).ToHashSet();
					var deletedWorkshopUUIDs = e.DeletedFiles.Where(x => x.IsWorkshop).Select(x => x.UUID).ToHashSet();
					ViewModel.RemoveDeletedMods(deletedUUIDs, deletedWorkshopUUIDs, e.RemoveFromLoadOrder);
					main.Activate();
				}
			};

			FocusManager.SetFocusedElement(this, this.MainContentPresenter);
		}

		public MainViewControl(MainWindow window, MainWindowViewModel vm)
        {
            InitializeComponent();

			main = window;
			ViewModel = vm;

			AlertBar.Show += AlertBar_Show;

			var res = this.TryFindResource("ModUpdaterPanel");
			if (res != null && res is ModUpdatesLayout modUpdaterPanel)
			{
				var binding = new Binding("ModUpdatesViewData")
				{
					Source = ViewModel
				};
				modUpdaterPanel.SetBinding(ModUpdatesLayout.DataContextProperty, binding);
			}
		}
    }
}
