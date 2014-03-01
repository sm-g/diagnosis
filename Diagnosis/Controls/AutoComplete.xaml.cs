using Diagnosis.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for AutoComplete.xaml
    /// </summary>
    public partial class AutoComplete : UserControl
    {
        AutoCompleteViewModel vm
        {
            get
            {
                return (DataContext ?? (DataContext = new AutoCompleteViewModel())) as AutoCompleteViewModel;
            }
        }

        public AutoComplete()
        {
            InitializeComponent();

            DataContext = new AutoCompleteViewModel();
        }

        private void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowSuggestionsPopup();
        }

        private void input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && suggestions.SelectedIndex > 0)
            {
                suggestions.SelectedIndex--;
            }
            if (e.Key == Key.Down && suggestions.SelectedIndex < suggestions.Items.Count - 1)
            {
                suggestions.SelectedIndex++;
            }
        }

        private void suggestions_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    AddSuggestion();
                    break;

                case Key.Escape:
                    input.Focus();
                    break;
            }
        }
        private void input_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void input_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (vm.IsSymptomCompleted)
                    {
                        vm.Clear();
                    }
                    else
                    {
                        AddSuggestion();
                    }
                    break;
            }
        }

        private void ShowSuggestionsPopup()
        {
            Rect placementRect;
            if (input.CaretIndex != 0)
            {
                placementRect = input.GetRectFromCharacterIndex(input.CaretIndex, true);
            }
            else
            {
                placementRect = new Rect(1, 1, 0, input.ActualHeight);
            }
            popup.PlacementTarget = input;
            popup.PlacementRectangle = placementRect;

            popup.IsOpen = suggestions.Items.Count > 0;
        }


        private void AddSuggestion()
        {
            vm.Accept();

            input.CaretIndex = input.Text.Length;
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowSuggestionsPopup();
        }

        private void input_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ShowSuggestionsPopup();
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddSuggestion();
            input.Focus();
        }
    }
}