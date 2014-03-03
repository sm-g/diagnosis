using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    public partial class TileView : UserControl
    {
        public TileView()
        {
            InitializeComponent();

            search.ResultItemClicked += search_ResultItemClicked;
        }

        void BeginSearch()
        {
            search.DataContext = new SearchViewModel((explorer.DataContext as SymptomExplorerViewModel).CurrentSymptom);
            search.Visibility = System.Windows.Visibility.Visible;
            search.Focus();
        }

        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new Diagnosis.ViewModels.SymptomExplorerViewModel(Diagnosis.DataCreator.Symptoms);
        }

        private void EndSearch()
        {
            search.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
            var searchVM = search.DataContext as SearchViewModel;
            var symptomVM = (explorer.DataContext as SymptomExplorerViewModel).CurrentSymptom;
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                EndSearch();
            }
            if (e.Key == Key.Enter)
            {
                OnResultItemSelected();
            }
        }

        private void search_ResultItemClicked(object sender, System.EventArgs e)
        {
            OnResultItemSelected();
        }

        private void OnResultItemSelected()
        {
            var searchVM = search.DataContext as SearchViewModel;
            var symptomVM = this.DataContext as SymptomViewModel;
            if (symptomVM.AllChildren.SingleOrDefault(child => child == searchVM.SelectedItem) == null)
            {
                symptomVM.Add(searchVM.SelectedItem);
            }

            if (searchVM.SelectedItem != null)
                searchVM.SelectedItem.ToggleChecked();

            searchVM.Clear();
        }

        private void explorer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (up.Command.CanExecute(null))
                {
                    up.Command.Execute(null);
                }
            }
            if (e.Key == Key.Insert || e.Key == Key.F && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                BeginSearch();
            }
        }
    }
}