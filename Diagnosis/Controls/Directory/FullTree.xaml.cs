﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class FullTree : UserControl
    {
        public FullTree()
        {
            InitializeComponent();
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            tree.DataContext = Diagnosis.DataCreator.Symptoms;
        }
    }
}