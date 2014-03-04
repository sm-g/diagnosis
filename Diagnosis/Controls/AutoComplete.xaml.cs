using Diagnosis.Helpers;
using Diagnosis.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for AutoComplete.xaml
    /// </summary>
    public partial class AutoComplete : UserControl
    {
        private bool focusFromPopup;

        private AutoCompleteViewModel vm
        {
            get
            {
                return (autocomplete.DataContext ?? (autocomplete.DataContext = new AutoCompleteViewModel())) as AutoCompleteViewModel;
            }
        }

        public AutoComplete()
        {
            InitializeComponent();

            autocomplete.DataContext = new AutoCompleteViewModel();
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
                case Key.Escape:
                    HidePopup();
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


        private void HidePopup()
        {
            popup.IsOpen = false;
        }

        private void AddSuggestion()
        {
            vm.Accept();

            input.CaretIndex = input.Text.Length;
            ShowSuggestionsPopup();
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!focusFromPopup)
            {
                vm.Reset();
            }
            focusFromPopup = false;
            ShowSuggestionsPopup();
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(autocomplete) && FocusChecker.IsFocusOutsideDepObject(popup.Child))
            {
                HidePopup();
            }
        }

        private void input_GotMouseCapture(object sender, MouseEventArgs e)
        {
            vm.Reset();
            ShowSuggestionsPopup();
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddSuggestion();
            focusFromPopup = true;
            input.Focus();
        }

        private void suggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            suggestions.ScrollIntoView(suggestions.SelectedItem);
        }
    }
}