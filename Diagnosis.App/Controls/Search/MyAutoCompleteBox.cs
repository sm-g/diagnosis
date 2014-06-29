using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace Diagnosis.App.Controls
{
    public class MyAutoCompleteBox : AutoCompleteBox
    {
        TextBox _textBox;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!IsDropDownOpen && SelectedItem != null && (e.Key == Key.Enter || e.Key == Key.Return))
            {
                // Drop down is closed so the item in the textbox should be submitted with a press of the Enter key
                base.OnKeyDown(e); // This has to happen before we mark Handled = false
                e.Handled = false; // Set Handled = false so the event bubbles up to your inputbindings
                return;
            }

            // Drop down is open so user must be selecting an AutoComplete list item
            base.OnKeyDown(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Template == null)
                return;
            _textBox = Template.FindName("Text", this) as TextBox;
            _textBox.SelectionChanged += _textBox_SelectionChanged;
        }

        void _textBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            Debug.WriteLine("CaretIndex = {0}", CaretIndex);
        }
        public void Select(int start, int length)
        {
            if (_textBox == null)
                return;
            _textBox.Select(start, length);
        }

        public int CaretIndex
        {
            get
            {
                return _textBox != null ? _textBox.CaretIndex : -1;
            }
            set
            {
                if (_textBox != null)
                {
                    _textBox.CaretIndex = value;
                }
            }
        }

    }
}
