using Diagnosis.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls.Forms
{
    public partial class DateOffsetPicker : UserControl
    {

        public DateOffset DateOffset
        {
            get { return (DateOffset)GetValue(DateOffsetProperty); }
            set { SetValue(DateOffsetProperty, value); }
        }

        public static readonly DependencyProperty DateOffsetProperty =
            DependencyProperty.Register("DateOffset", typeof(DateOffset), typeof(DateOffsetPicker));

        public DateOffsetPicker()
        {
            InitializeComponent();
        }

    }
}
