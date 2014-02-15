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
    /// Interaction logic for TileItem.xaml
    /// </summary>
    public partial class TileItem : UserControl
    {
        public TileItem()
        {
            InitializeComponent();
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

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                (DataContext as SymptomViewModel).ToggleChecked();
            }
            if (e.Key == Key.F2)
            {
                BeginEdit();
            }
            if (e.Key == Key.Enter)
            {
                CommitChanges();
            }
            if (e.Key == Key.Escape)
            {
                RevertChanges();
            }
        }
    }
}
