using Diagnosis.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class FullTree : TreeCommon
    {

        public FullTree()
        {
            InitializeComponent();
        }
        private void item_selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi != null)
                tvi.BringIntoView();
            e.Handled = true;
        }
    }
}