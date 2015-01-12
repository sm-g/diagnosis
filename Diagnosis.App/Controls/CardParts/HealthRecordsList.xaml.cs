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
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (records.SelectedItems.Cast<object>().Where(i => i == item).Count() > 1)
                    {
                        records.SelectedItems.Remove(item);
                    }
                }
            }

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

            //logger.DebugFormat("records got focus");

            // список получил фокус - переводим его на выбранный элемент
            var index = records.SelectedIndex;
            if (index >= 0)
            {
                Action action = () =>
                {
                    records.ScrollIntoView(records.SelectedItem);

                    var item = records.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
                    if (item != null)
                    {
                        var scope = FocusManager.GetFocusScope(this);
                        FocusManager.SetFocusedElement(scope, item);
                        // item.Focus();
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