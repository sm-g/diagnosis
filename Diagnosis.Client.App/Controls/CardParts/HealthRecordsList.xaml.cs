using Diagnosis.ViewModels.Screens;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Diagnosis.Client.App.Controls.CardParts
{
    public partial class HealthRecordsList : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordsList));

        private HrListViewModel Vm { get { return DataContext as HrListViewModel; } }

        public HealthRecordsList()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                debugFocus.IsChecked = true;
#if !DEBUG
                debugFocus.IsChecked = false;
                debug.Visibility = System.Windows.Visibility.Collapsed;
                //recordsNone.Visibility = System.Windows.Visibility.Collapsed;
                //recordsOrder.Visibility = System.Windows.Visibility.Collapsed;
                //recordsGrid.ColumnDefinitions.RemoveAt(1);
                //recordsGrid.ColumnDefinitions.RemoveAt(1);
#endif
                records.SelectionChanged += records_SelectionChanged;
            };
            records.GotKeyboardFocus += records_GotKeyboardFocus;
            records.GotFocus += records_GotFocus;
        }

        private void records_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // fix duplicates in SelectedItems http://stackoverflow.com/questions/27685460/wpf-listbox-extendedmode-move-selected-item-with-isselected-binding
            Vm.inRemoveDup = true;
            foreach (var item in e.AddedItems)
            {
                if (records.SelectedItems.Cast<object>().Where(i => i == item).Count() > 1)
                {
                    // logger.DebugFormat("records remove duplicate {0}", item);
                    records.SelectedItems.Remove(item);
                    //(item as CheckableBase).IsSelected = true;
                }
            }
            Vm.inRemoveDup = false;

            if (records.SelectedItems.Count > 0)
            {
                var lastSelectedItem = records.ItemContainerGenerator.ContainerFromItem(records.SelectedItems[records.SelectedItems.Count - 1]) as ListBoxItem;
                var selectedItem = records.ItemContainerGenerator.ContainerFromItem(records.SelectedItem) as ListBoxItem;
#if DEBUG
                //var marker = lastSelectedItem.FindChild<Rectangle>("marker");
                //if (marker != null)
                //{
                //    marker.Visibility = (lastSelectedItem == selectedItem ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
                //}
#endif

                // focus on selected item if listbox itself is focused
                if (Keyboard.FocusedElement == records)
                {
                    if (selectedItem != null)
                    {
                        //logger.DebugFormat("records in focus, focus selected {0}", selectedItem);
                        selectedItem.Focus();
                    }
                }
            }
            //logger.DebugFormat("records SelectionChanged {0},\n add {1},\n remove {2}",
            //    records.SelectedItem,
            //    string.Join("\n", e.AddedItems.Cast<object>()),
            //    string.Join("\n", e.RemovedItems.Cast<object>()));

            if (!Vm.inRemoveDup)
                try
                {
                    records.UpdateLayout();
                    records.ScrollIntoView(records.SelectedItem);
                }
                catch (InvalidOperationException)
                {
                    // may be StartAt
                }
        }

        private void records_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var selectedVm = Vm.SelectedHealthRecord;

            var item = records.SelectedItem;//== null без синхронизации
            item = item ?? (records.Items.Count > 0 ? records.Items.GetItemAt(0) : null);

            //logger.DebugFormat("records got focus\nNewFocus {2},\nSelectedItem {0}\nitem[0]{3}\nvm SelectedHealthRecord {1}\nLastSelected {4}",
            //    records.SelectedItem,
            //    selectedVm,
            //    e.NewFocus,
            //    records.Items.GetItemAt(0),
            //    Vm.LastSelected);

            if (e.NewFocus != records)
                return;

            // список получил фокус - переводим его на выбранный элемент или первый (если элементов нет, список не виден)
            if (item != null)
            {
                Action action = () =>
                {
                    records.ScrollIntoView(records.SelectedItem);

                    var lbItem = records.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    if (lbItem != null && !Vm.inManualFocusSetting)
                    {
                        logger.DebugFormat("end set focus to {0}", item);
                        var scope = FocusManager.GetFocusScope(this);
                        FocusManager.SetFocusedElement(scope, lbItem);
                    }
                    Vm.inManualFocusSetting = false;
                };

                logger.DebugFormat("begin set focus to {0}", item);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }

        private void records_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //logger.DebugFormat("records got L focus");
        }
    }
}