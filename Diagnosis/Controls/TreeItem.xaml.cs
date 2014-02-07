using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diagnosis.ViewModels;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeItem.xaml
    /// </summary>
    public partial class TreeItem : UserControl
    {
        //List<Control> editor = new List<Control>();

        public TreeItem()
        {
            InitializeComponent();
            //editor.Add(titleEditor);
            //editor.Add(isGroup);
        }

        internal void ToggleCheckedState()
        {
            if (DataContext is SymptomViewModel)
            {
                (DataContext as SymptomViewModel).ToggleChecked();
            }
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

        void ShowEditor()
        {
            editor.Visibility = System.Windows.Visibility.Visible;
        }

        void HideEditor()
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
