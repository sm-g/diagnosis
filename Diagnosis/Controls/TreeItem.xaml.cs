using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeItem.xaml
    /// </summary>
    public partial class TreeItem : UserControl
    {
        public TreeItem()
        {
            InitializeComponent();
        }

        internal void ToggleCheckedState()
        {
            (DataContext as SymptomViewModel).ToggleChecked();
        }

        internal void BeginEdit()
        {
            ShowEditor();
            titleEditor.Focus();
        }

        internal void CommitChanges()
        {
            HideEditor();
        }

        internal void RevertChanges()
        {
            titleEditor.Text = title.Text;
            HideEditor();
        }

        internal void BeginSearch()
        {
            search.DataContext = new SearchViewModel(this.DataContext as SymptomViewModel);
            search.Visibility = System.Windows.Visibility.Visible;
            search.Focus();
        }

        //internal void CheckSearchResult(SymptomViewModel)
        //{
        //    (DataContext as SymptomViewModel).IsChecked = true;
        //}

        private void ShowEditor()
        {
            editor.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideEditor()
        {
            editor.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void editor_LostFocus(object sender, RoutedEventArgs e)
        {
            var element = FocusManager.GetFocusedElement(Application.Current.MainWindow);

            if (editor.Children.IndexOf((UIElement)element) == -1)
            {
                CommitChanges();
            }
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                search.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (e.Key == Key.Space)
            {
            }
        }
    }
}