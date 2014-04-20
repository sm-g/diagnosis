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
using System.Globalization;

namespace Diagnosis.App.Controls.Forms
{
    /// <summary>
    /// Interaction logic for ComboBoxDatePicker.xaml
    /// </summary>
    public partial class ComboBoxDatePicker : UserControl
    {
        string[] days;
        string[] monthNames;
        List<string> years = new List<string>();

        public int? Year
        {
            get { return (int?)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(int?), typeof(ComboBoxDatePicker));

        public int? Month
        {
            get { return (int?)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }

        public static readonly DependencyProperty MonthProperty =
            DependencyProperty.Register("Month", typeof(int?), typeof(ComboBoxDatePicker), new PropertyMetadata(13));

        public int? Day
        {
            get { return (int?)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(int?), typeof(ComboBoxDatePicker));


        public int YearsDepth
        {
            get { return (int)GetValue(YearsDepthProperty); }
            set { SetValue(YearsDepthProperty, value); }
        }

        public static readonly DependencyProperty YearsDepthProperty =
            DependencyProperty.Register("YearsDepth", typeof(int), typeof(ComboBoxDatePicker), new PropertyMetadata(120));

        public ComboBoxDatePicker()
        {
            InitializeComponent();

            monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames.ToArray();
            years = Enumerable.Range(DateTime.Now.Year - YearsDepth, YearsDepth + 1).Select(y => y.ToString()).ToList();
            years.Add("");
            years.Reverse();

            LoadYearsCombo();

            LoadMonthsCombo();
        }

        private void LoadYearsCombo()
        {
            comboYears.ItemsSource = years;
            if (Year.HasValue)
            {
                var ys = Year.ToString();
                if (years.Contains(ys))
                    comboYears.SelectedValue = ys;
            }
            else
            {
                comboYears.SelectedValue = "";
            }
        }

        private void LoadMonthsCombo()
        {
            comboMonths.ItemsSource = monthNames;
            comboMonths.SelectedValue = DateTimeFormatInfo.CurrentInfo.GetMonthName(Month.Value);
        }

        private void LoadDaysCombo()
        {
            comboDays.ItemsSource = days;
            if (Day.HasValue)
            {
                var ds = Day.ToString();
                if (days.Contains(ds))
                    comboDays.SelectedValue = ds;
            }
            else
            {
                comboDays.SelectedValue = "";
            }
        }

        private void InitDays()
        {
            int daysInMonth = 31;
            try
            {
                daysInMonth = DateTime.DaysInMonth(Year.Value, Month.Value);
                // exception when month or year not set
            }
            catch
            {
            }
            days = new string[daysInMonth + 1];
            days[0] = "";
            for (int i = 1; i < days.Length - 1; i++)
            {
                days[i] = i.ToString();
            }
            if (Day > daysInMonth)
            {
                Day = daysInMonth;
            }
        }

        private void comboYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            InitDays();
            LoadDaysCombo();
        }

        private void comboMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            InitDays();
            LoadDaysCombo();
        }
    }
}
