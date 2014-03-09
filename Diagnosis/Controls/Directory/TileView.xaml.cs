﻿using Diagnosis.ViewModels;
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
        }

        void BeginSearch()
        {
            search.Visibility = System.Windows.Visibility.Visible;

            search.Focus();
        }

        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            explorer.DataContext = new Diagnosis.ViewModels.SymptomExplorer(Diagnosis.DataCreator.Symptoms);
        }

        private void EndSearch()
        {
            search.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                EndSearch();
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