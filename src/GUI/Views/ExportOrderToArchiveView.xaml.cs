using DivinityModManager.Converters;
using DivinityModManager.ViewModels;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DivinityModManager.Views
{
	public class ExportOrderToArchiveViewViewBase : ReactiveUserControl<ExportOrderToArchiveViewModel> { }

	/// <summary>
	/// Interaction logic for ExportOrderToArchiveView.xaml
	/// </summary>
	public partial class ExportOrderToArchiveView : ExportOrderToArchiveViewViewBase
	{
		public ExportOrderToArchiveView()
		{
			InitializeComponent();

			ViewModel = new ExportOrderToArchiveViewModel();
			DataContext = ViewModel;

			this.WhenActivated(d =>
			{
				if (this.ViewModel != null)
				{
					d(this.OneWayBind(ViewModel, vm => vm.IsVisible, view => view.Visibility, BoolToVisibilityConverter.FromBool));

					d(this.OneWayBind(ViewModel, vm => vm.Entries, v => v.FilesListView.ItemsSource));

					d(this.OneWayBind(ViewModel, vm => vm.IsRunning, v => v.ProgressIndicator.IsBusy));
					d(this.OneWayBind(ViewModel, vm => vm.ProgressTitle, v => v.TaskProgressTitleText.Text));
					d(this.OneWayBind(ViewModel, vm => vm.ProgressWorkText, v => v.TaskProgressWorkText.Text));
					d(this.OneWayBind(ViewModel, vm => vm.ProgressValue, v => v.TaskProgressBar.Value));
					d(this.OneWayBind(ViewModel, vm => vm.IsProgressActive, view => view.TaskProgressBar.Visibility, BoolToVisibilityConverter.FromBool));

					d(this.Bind(ViewModel, vm => vm.IncludeOverrides, view => view.IncludeOverridesCheckbox.IsChecked));
					d(this.Bind(ViewModel, vm => vm.SelectedOrderType, view => view.OrderTypeComboBox.SelectedItem));
					d(this.Bind(ViewModel, vm => vm.OrderTypes, view => view.OrderTypeComboBox.ItemsSource));

					//d(this.BindCommand(ViewModel, vm => vm.SelectAllCommand, v => v.ConfirmButton));
					d(this.BindCommand(ViewModel, vm => vm.RunCommand, v => v.ConfirmButton));
					d(this.BindCommand(ViewModel, vm => vm.CancelRunCommand, v => v.CancelProgressButton));
					d(this.BindCommand(ViewModel, vm => vm.CloseCommand, v => v.CancelButton));
				}
			});
		}
	}
}
