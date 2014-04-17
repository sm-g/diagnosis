using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diagnosis.App.Controls
{
    /// <summary>
    /// Interaction logic for RadioListBox.xaml
    /// </summary>
    public partial class RadioListBox : ListBox
    {
        public RadioListBox()
        {
            InitializeComponent();

            SelectionMode = SelectionMode.Single;
        }

        public new SelectionMode SelectionMode
        {
            get
            {
                return base.SelectionMode;
            }
            private set
            {
                base.SelectionMode = value;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            CheckRadioButtons(e.RemovedItems, false);
            CheckRadioButtons(e.AddedItems, true);
        }

        private void CheckRadioButtons(System.Collections.IList radioButtons, bool isChecked)
        {
            foreach (object item in radioButtons)
            {
                ListBoxItem lbi = this.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;

                if (lbi != null)
                {
                    RadioButton radio = lbi.Template.FindName("radio", lbi) as RadioButton;
                    if (radio != null)
                        radio.IsChecked = isChecked;
                }
            }
        }
    }
}
