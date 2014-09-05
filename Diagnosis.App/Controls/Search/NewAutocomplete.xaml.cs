﻿using System;
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
using Diagnosis.ViewModels.Search.Autocomplete;

namespace Diagnosis.App.Controls.Search
{
    /// <summary>
    /// Interaction logic for NewAutocomplete.xaml
    /// </summary>
    public partial class NewAutocomplete : UserControl
    {
        private bool focusFromPopup;
        Autocomplete Vm { get { return DataContext as Autocomplete; } }

        public NewAutocomplete()
        {
            InitializeComponent();
        }

        private void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && suggestions.SelectedIndex > 0)
            {
                suggestions.SelectedIndex--;
            }
            else if (e.Key == Key.Down && suggestions.SelectedIndex < suggestions.Items.Count - 1)
            {
                suggestions.SelectedIndex++;
            }
            else if (e.Key == Key.Escape)
            {
                if (popup.IsOpen)
                {
                    //e.Handled = true;
                    // повторное нажатие закроет запись
                }
                HidePopup();
            }
        }

        private void HidePopup()
        {
            popup.IsOpen = false;
        }
        private void EnterFormPopup()
        {
            Vm.EnterCommand.Execute(null);
            focusFromPopup = true;
            input.Focus();
        }

        private void suggestions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                EnterFormPopup();
            }
        }

        private void suggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            suggestions.ScrollIntoView(suggestions.SelectedItem);
        }

        private void suggestions_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                HidePopup();
                focusFromPopup = true;
                input.Focus();
            }
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}