using Diagnosis.ViewModels.Search.Autocomplete;
using log4net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.Search
{
    public partial class Autocomplete : UserControl
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AutocompleteViewModel));

        private AutocompleteViewModel Vm { get { return DataContext as AutocompleteViewModel; } }

        public Autocomplete()
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
                popup.IsOpen = false;
            }
            else if (e.Key == Key.LeftShift)
            {
                Vm.ShowAltSuggestion = true;
            }
        }

        private void input_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                Vm.ShowAltSuggestion = false;
            }
        }

        private void suggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            suggestions.ScrollIntoView(suggestions.SelectedItem);
        }

        private void input_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var element = input.ItemContainerGenerator.ContainerFromItem(e.AddedItems[e.AddedItems.Count - 1]) as UIElement;
                popup.PlacementTarget = element;
            }
        }

        #region focus stuff

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            // logger.Debug("autocomplete got focus");
        }

        /// <summary>
        /// Фокус ушел из автокомплита (из тега или попапа) - завершаем тег.
        /// </summary>
        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            var outs = IsFocusOutside();
            //logger.DebugFormat("autocomplete lost focus to {0}, out? {1}", GetFocusedInScope(), outs);
            if (outs)
            {
                //logger.Debug("autocomplete lost focus");
                // if (Vm != null && Vm.EditingTag != null)
                //       Vm.CompleteOnLostFocus(Vm.EditingTag); // также в HrEditor.CloseCurrentHr()
            }
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var outs = IsFocusOutside();
            // logger.DebugFormat("autocomplete lost kb focus to {0}, out? {1}", e.NewFocus, outs);
            if (outs)
            {
                logger.Debug("autocomplete lost focus");
                if (Vm != null && Vm.EditingTag != null)
                    Vm.CompleteOnLostFocus(Vm.EditingTag); // также в HrEditor.CloseCurrentHr()
            }
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            // logger.Debug("input got focus");
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            // logger.DebugFormat("input lost focus, {0} {1}", e.OriginalSource, e.Source);
        }

        private void popup_GotFocus(object sender, RoutedEventArgs e)
        {
            // logger.Debug("popup got focus");
        }

        private void popup_LostFocus(object sender, RoutedEventArgs e)
        {
            // logger.DebugFormat("popup lost focus to {0}", GetFocusedInScope());
        }

        private void Tag_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //var fromSugggsetions = InSuggsetions(e.OldFocus as DependencyObject);
            //logger.DebugFormat("tag got focus, fromPopup={0}", fromSugggsetions);
        }

        /// <summary>
        /// Фокус ввода ушел с тега в попап — тег не завершаем, иначе — убираем попап.
        /// </summary>
        private void Tag_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var toSugggsetions = InSuggsetions(e.NewFocus as DependencyObject);
            logger.DebugFormat("tag lost focus, toPopup={0}", toSugggsetions);
            if (toSugggsetions)
            {
                Vm.CanCompleteOnLostFocus = false;
            }
            else
            {
                popup.IsOpen = false;
            }
        }

        private void Tag_LostFocus(object sender, RoutedEventArgs e)
        {
            //logger.DebugFormat("tag lost locigfocus");
        }

        // helpers

        private bool InSuggsetions(DependencyObject dep)
        {
            return ParentFinder.FindAncestorOrSelf<ListBox>(dep) == suggestions;
        }

        private bool IsFocusOutside()
        {
            return FocusChecker.IsFocusOutsideDepObject(this) && FocusChecker.IsFocusOutsideDepObject(popup.Child);
        }

        private IInputElement GetFocusedInScope()
        {
            return FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this));
        }

        #endregion focus stuff
    }
}