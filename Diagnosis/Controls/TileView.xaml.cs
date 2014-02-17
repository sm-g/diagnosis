using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        }

        internal void BeginSearch()
        {
            search.DataContext = new SearchViewModel((explorer.DataContext as SymptomExplorerViewModel).CurrentSymptom);
            search.Visibility = System.Windows.Visibility.Visible;
            search.Focus();
        }

        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new Diagnosis.ViewModels.SymptomExplorerViewModel(Diagnosis.DataCreator.CreateSymptoms());
        }

        private void EndSearch()
        {
            search.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
            var searchVM = search.DataContext as SearchViewModel;
            var symptomVM = (explorer.DataContext as SymptomExplorerViewModel).CurrentSymptom;

            if (e.Key == Key.Escape)
            {
                EndSearch();
            }
            if (e.Key == Key.Enter)
            {
                if (symptomVM.Children.IndexOf(searchVM.SelectedItem) == -1)
                {
                    symptomVM.Add(searchVM.SelectedItem);
                }

                if (searchVM.SelectedItem != null)
                    searchVM.SelectedItem.ToggleChecked();

                searchVM.Clear();
            }
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