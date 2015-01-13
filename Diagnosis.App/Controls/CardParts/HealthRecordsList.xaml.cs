using Diagnosis.ViewModels.Screens;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.App.Controls.CardParts
{
    public partial class HealthRecordsList : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordsList));
        public HealthRecordsList()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if !DEBUG
                debug.Visibility = System.Windows.Visibility.Collapsed;
#endif
            };
        }

        private void records_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // fix duplicates in SelectedItems http://stackoverflow.com/questions/27685460/wpf-listbox-extendedmode-move-selected-item-with-isselected-binding

            foreach (var item in e.AddedItems)
            {
                if (records.SelectedItems.Cast<object>().Where(i => i == item).Count() > 1)
                {
                    records.SelectedItems.Remove(item);
                }
            }

            // focus on selected item if listbox itself is focused
            if (Keyboard.FocusedElement == records)
            {
                if (records.SelectedItems.Count > 0)
                {
                    var lastSelectedItem = records.ItemContainerGenerator.ContainerFromItem(records.SelectedItems[records.SelectedItems.Count - 1]) as ListBoxItem;
                    if (lastSelectedItem != null)
                        lastSelectedItem.Focus();
                }
            }

            logger.DebugFormat("records selected {0}", records.SelectedItem);
            records.UpdateLayout();
            records.ScrollIntoView(records.SelectedItem);
        }

        private void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                // e.Handled = true;
            }
            ;
        }

        private void records_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus != records)
                return;

            // список получил фокус - переводим его на выбранный элемент
            var item = records.SelectedItem;
            // logger.DebugFormat("records got focus, selected {0}", item);
            if (item != null)
            {
                Action action = () =>
                {
                    records.ScrollIntoView(records.SelectedItem);

                    var lbItem = records.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    if (lbItem != null)
                    {
                        //var scope = FocusManager.GetFocusScope(this);
                        //FocusManager.SetFocusedElement(scope, lbItem);
                        lbItem.Focus();
                    }
                };
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }

        private void records_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            logger.DebugFormat("records got L focus");
        }
    }
}