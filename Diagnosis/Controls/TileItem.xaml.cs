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
    public partial class TileItem : UserControl, IEditableItem
    {
        public TileItem()
        {
            InitializeComponent();
        }

        public void BeginEdit()
        {
            ShowEditor();
            titleEditor.Focus();
        }

        public void CommitChanges()
        {
            EndEdit();
        }

        public void RevertChanges()
        {
            titleEditor.Text = title.Text;
            EndEdit();
        }

        private void ShowEditor()
        {
            editor.Visibility = System.Windows.Visibility.Visible;
        }

        private void EndEdit()
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
