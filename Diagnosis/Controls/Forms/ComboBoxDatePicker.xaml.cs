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

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for ComboBoxDatePicker.xaml
    /// </summary>
    public partial class ComboBoxDatePicker : UserControl
    {
        int[] Days = new int[31];
        string[] monthNames;
        List<int> years = new List<int>();

        public int Year
        {
            get { return (int)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(int), typeof(ComboBoxDatePicker), new PropertyMetadata(DateTime.Now.Year));

        public int Month
        {
            get { return (int)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }

        public static readonly DependencyProperty MonthProperty =
            DependencyProperty.Register("Month", typeof(int), typeof(ComboBoxDatePicker), new PropertyMetadata(13));

        public int Day
        {
            get { return (int)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(int), typeof(ComboBoxDatePicker), new PropertyMetadata(DateTime.Now.Day));


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
            years = Enumerable.Range(DateTime.Now.Year - YearsDepth, YearsDepth + 1).ToList();

            LoadYearsCombo();

            LoadMonthsCombo();
        }

        private void LoadYearsCombo()
        {
            comboYears.ItemsSource = years;
            comboYears.SelectedValue = Year;
        }

        private void LoadMonthsCombo()
        {
            comboMonths.ItemsSource = monthNames;
            comboMonths.SelectedValue = DateTimeFormatInfo.CurrentInfo.GetMonthName(Month);
        }

        private void LoadDaysCombo()
        {
            comboDays.ItemsSource = Days;
            comboDays.SelectedValue = Day;
        }

        private void InitDays()
        {
            if (Month == 13)
            {
                Days = new int[0];
            }
            else
            {
                var daysInMonth = DateTime.DaysInMonth(Year, Month);
                Days = new int[DateTime.DaysInMonth(Year, Month)];
                for (int i = 0; i < Days.Count(); i++)
                {
                    Days[i] = i + 1;
                }
                if (Day > daysInMonth)
                {
                    Day = daysInMonth;
                }
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
