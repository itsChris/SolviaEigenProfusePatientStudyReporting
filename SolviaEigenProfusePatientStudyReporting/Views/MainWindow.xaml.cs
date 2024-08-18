using SolviaEigenProfusePatientStudyReporting.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace SolviaEigenProfusePatientStudyReporting.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (sender is DataGrid dataGrid && dataGrid.SelectedCells.Count > 0)
                {
                    var cellInfo = dataGrid.SelectedCells[0];
                    var content = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock)?.Text;
                    Clipboard.SetText(content ?? string.Empty);

                    Log.Verbose("Copied content to clipboard: {Content}", content);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while copying content to the clipboard");
                MessageBox.Show("An error occurred while copying the content. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
