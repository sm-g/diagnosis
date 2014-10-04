using log4net;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls.Editors
{
    /// <summary>
    /// Interaction logic for ComboBoxDatePicker.xaml
    /// </summary>
    public partial class ComboBoxDatePicker : UserControl
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ComboBoxDatePicker));

        public ObservableCollection<string> Days
        {
            get;
            set;
        }

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
            Days = new ObservableCollection<string>();

            LoadYearsCombo();
            LoadMonthsCombo();
            LoadDaysCombo();
        }

        private void LoadYearsCombo()
        {
            var years = Enumerable.Range(DateTime.Now.Year - YearsDepth, YearsDepth + 1).Select(y => y.ToString()).ToList();
            years.Add("");
            years.Reverse();
            comboYears.ItemsSource = years;
        }

        private void LoadMonthsCombo()
        {
            var monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames.ToArray();
            comboMonths.ItemsSource = monthNames;
            comboMonths.SelectedValue = DateTimeFormatInfo.CurrentInfo.GetMonthName(Month ?? 13);
        }

        private void LoadDaysCombo()
        {
            comboDays.ItemsSource = Days;
        }

        private void FillDaysCombo()
        {
            var days = GetDaysComboItems();
            var daysInMonth = days.Count() - 1;

            var daysToAdd = days.Except(Days).ToList();
            var daysToRemove = Days.Except(days).ToList();

            // в новом месяце меньше дней - выбриаем последний
            if (Day > daysInMonth)
            {
                Day = daysInMonth;
            }

            daysToRemove.ForEach(d => Days.Remove(d));
            daysToAdd.ForEach(d => Days.Add(d));
        }

        /// <summary>
        /// '', '1', ..., '30' 
        /// </summary>
        /// <returns></returns>
        private string[] GetDaysComboItems()
        {
            var year = Year ?? DateTime.Today.Year;
            var month = Month != 13 && Month != null ? Month.Value : DateTime.Today.Month;
            var daysInMonth = DateTime.DaysInMonth(year, month);
            string[] days = new string[daysInMonth + 1];
            days[0] = "";
            for (int i = 1; i < days.Length; i++)
            {
                days[i] = i.ToString();
            }
            return days;
        }

        private void comboYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //logger.DebugFormat("year = {0}", Year);
            e.Handled = true;
            // смена года — было 29 февраля, меняем на 28
            FillDaysCombo();
        }

        private void comboMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //logger.DebugFormat("month = {0}", Month);
            e.Handled = true;
            // смена месяца — другой набор дней, если был день 31, меням на 30
            FillDaysCombo();
        }

        private void comboDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //logger.DebugFormat("day = {0}", Day);
        }

        private void comboYears_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Year == null)
                comboYears.SelectedIndex = 0;
        }
        private void comboDays_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Day == null)
                comboDays.SelectedIndex = 0;
        }

    }
}