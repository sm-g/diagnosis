using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;


namespace Diagnosis.Common.Styles
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
