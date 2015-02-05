using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Common.Presentation.Styles
{
    partial class TreeStyle : ResourceDictionary
    {
        public TreeStyle()
        {
            InitializeComponent();
        }

        private void item_selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            if (tvi != null)
            {
                tvi.BringIntoView();
                tvi.Focus();
            }
            e.Handled = true;
        }
    }
}