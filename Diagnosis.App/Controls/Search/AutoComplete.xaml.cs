using Diagnosis.Core;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for AutoComplete.xaml
    /// </summary>
    public partial class AutoComplete : UserControl
    {
        private bool focusFromPopup;
        IAutoComplete vm;

        public AutoComplete()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (vm != null)
            {
                vm.SuggestionAccepted -= vm_SuggestionAccepted;
            }
            vm = DataContext as IAutoComplete;
            if (vm != null)
            {
                vm.SuggestionAccepted += vm_SuggestionAccepted;
            }
        }

        private void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshPopup();

            // после курсора разделительный пробел
            if (input.Text.Length == input.CaretIndex + 1 && input.Text.Last() == vm.DelimSpacer)
            {
                input.CaretIndex = input.Text.Length;
            }
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
                    e.Handled = true;
                    // повторное нажатие закроет запись
                }
                HidePopup();
            }
        }

        private void RefreshPopup()
        {
            if (suggestions.Items.Count > 0
                && !string.IsNullOrWhiteSpace(input.Text))
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
                popup.PlacementRectangle = placementRect;
                popup.IsOpen = true;
            }
            else
            {
                HidePopup();
            }
        }

        private void HidePopup()
        {
            popup.IsOpen = false;
        }

        private void vm_SuggestionAccepted(object sender, AutoCompleteEventArgs e)
        {
            input.CaretIndex = input.Text.Length;
            HidePopup();
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!focusFromPopup)
            {
                // vm.Reset();
            }
            focusFromPopup = false;
            RefreshPopup();
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
            if (FocusChecker.IsFocusOutsideDepObject(autocomplete) && FocusChecker.IsFocusOutsideDepObject(popup.Child))
            {
                //vm.Reset();
            }
            RefreshPopup();
        }

        private void EnterFormPopup()
        {
            vm.EnterCommand.Execute(null);
            focusFromPopup = true;
            input.Focus();
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EnterFormPopup();
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
    }
}