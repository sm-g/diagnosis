using Diagnosis.ViewModels.Autocomplete;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls.CardParts
{
    public partial class HealthRecordEditor : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecordEditor));

        public HealthRecordEditor()
        {
            InitializeComponent();
            offsetSettings.Visibility = System.Windows.Visibility.Collapsed;
#if !DEBUG
#endif
        }

        private void grid_Drop(object sender, DragEventArgs e)
        {
            (autocomplete.DataContext as AutocompleteViewModel).OnDrop(e);
        }

        private void grid_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == grid && !e.OldFocus.IsChildOf(this))
            {
                //logger.DebugFormat("hreditor grid got Key focus, old {0}", e.OldFocus);
                // фокус пришел не из редактора
                autocomplete.Focus();
            }
        }

        private void grid_GotFocus(object sender, RoutedEventArgs e)
        {
            //logger.DebugFormat("hreditor grid got Logical focus");
        }

        private void grid_LostFocus(object sender, RoutedEventArgs e)
        {
            //logger.DebugFormat("hreditor grid lost Logical focus");
        }
    }
}