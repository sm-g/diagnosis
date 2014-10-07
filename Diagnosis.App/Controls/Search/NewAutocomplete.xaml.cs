using Diagnosis.ViewModels.Search.Autocomplete;
using log4net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.Search
{
    /// <summary>
    /// Interaction logic for NewAutocomplete.xaml
    /// </summary>
    public partial class NewAutocomplete : UserControl
    {
        public static readonly ILog logger = LogManager.GetLogger(typeof(Autocomplete));

        private Autocomplete Vm { get { return DataContext as Autocomplete; } }

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
                // нельзя выбирать предположения, когда попап скрыт (когда при пустом запросе показываем все предположения)
                suggestions.SelectedItem = null;

                popup.IsOpen = false;
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
            // logger.DebugFormat("autocomplete lost focus to {0}, out? {1}", GetFocusedInScope(), outs);

            if (outs)
            {
                logger.Debug("complete from xaml");
                if (Vm.EditingTag != null)
                    Vm.CompleteOnLostFocus(Vm.EditingTag);
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

        IInputElement GetFocusedInScope()
        {
            return FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this));
        }
        #endregion

    }
}