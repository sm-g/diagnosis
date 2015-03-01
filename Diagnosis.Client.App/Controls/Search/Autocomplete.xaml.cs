using Diagnosis.Common.Presentation.Controls;
using Diagnosis.ViewModels.Autocomplete;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.Client.App.Controls.Search
{
    public partial class Autocomplete : UserControl
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AutocompleteViewModel));

        private AutocompleteViewModel Vm { get { return DataContext as AutocompleteViewModel; } }

        public Autocomplete()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if DEBUG
                var setterBorderThickness = new Setter(Control.BorderThicknessProperty, new Thickness(0));
                var setterBorderBrush = new Setter(Control.BorderBrushProperty, SystemColors.HighlightBrush);
                // толстая рамка когда фокус на автокомплите
                var setter = new Setter(Control.BorderThicknessProperty, new Thickness(4));
                var isFocused = new Binding("IsFocused")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                };
                var trigger = new DataTrigger()
                {
                    Binding = isFocused,
                    Value = true,
                    Setters = { setter }
                };
                this.Style = new Style(typeof(UserControl))
                {
                    Setters = { setterBorderThickness, setterBorderBrush },
                    Triggers = { trigger }
                };
#endif
            };
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
            // перемещаем popup
            if (e.AddedItems.Count > 0)
            {
                var element = input.ItemContainerGenerator.ContainerFromItem(e.AddedItems[e.AddedItems.Count - 1]) as UIElement;
                popup.PlacementTarget = element;
            }
        }

        #region focus stuff

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            var outs = IsFocusOutside();
            //logger.DebugFormat("autocomplete lost focus to {0}, out? {1}", GetFocusedInScope(), outs);
        }

        private void autocomplete_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == autocomplete)
            {
                Vm.StartEdit();
            }
        }

        /// <summary>
        /// Фокус ушел из автокомплита (из тега или попапа) - завершаем тег.
        /// </summary>
        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var outs = IsFocusOutside();
            logger.DebugFormat("autocomplete lost kb focus to {0}, out? {1}", e.NewFocus, outs);
            if (outs)
            {
                if (Vm != null && Vm.EditingTag != null)
                    Vm.CompleteOnLostFocus(Vm.EditingTag); // также в HrEditor.CloseCurrentHr()
            }
        }

        private void input_GotFocus(object sender, RoutedEventArgs e)
        {
            // logger.Debug("input got focus");
        }

        private void input_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // got focus after ctrl + j

            if (e.NewFocus == input)
            {
                if (Vm.InDispose)
                {
                    // autocomplete in dispose
                    // wait for new DataContext setted
                    DependencyPropertyChangedEventHandler a = null;
                    a = (s, args) =>
                    {
                        if (Vm != null && Keyboard.FocusedElement == input)
                        {
                            //logger.DebugFormat("StartEdit after DataContextChanged");
                            Vm.StartEdit(Vm.LastTag);
                        }
                        this.DataContextChanged -= a;
                    };
                    this.DataContextChanged += a;
                }
                else if (Vm.EditingTag != null && !Vm.InDragDrop)
                {
                    Vm.StartEdit(Vm.EditingTag);
                }
            }
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
            var toSugggsetions = e.NewFocus.IsChildOf(suggestions) || e.NewFocus == suggestions;
            //logger.DebugFormat("tag lost focus, toSugggsetions={0}", toSugggsetions);
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

        private bool IsFocusOutside()
        {
            return FocusChecker.IsLogicFocusOutside(this) && FocusChecker.IsLogicFocusOutside(popup.Child);
        }

        private IInputElement GetFocusedInScope()
        {
            return FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this));
        }

        #endregion focus stuff
    }
}