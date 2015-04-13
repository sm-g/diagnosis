using Diagnosis.Common;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.FormParts
{
    /// <summary>
    /// Interaction logic for ComboBoxDatePicker.xaml
    /// </summary>
    public partial class ComboBoxDatePicker : UserControl
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ComboBoxDatePicker));
        private bool inSelectedDateChanged;

        public ObservableCollection<string> Days
        {
            get;
            set;
        }

        public bool WithPicker
        {
            get { return (bool)GetValue(WithPickerProperty); }
            set { SetValue(WithPickerProperty, value); }
        }

        public static readonly DependencyProperty WithPickerProperty =
            DependencyProperty.Register("WithPicker", typeof(bool), typeof(ComboBoxDatePicker), new PropertyMetadata(false));

        public int? Year
        {
            get { return (int?)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(int?), typeof(ComboBoxDatePicker),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnYearChanged)
            // new CoerceValueCallback(CoerceYear)
                    ));

        private static void OnYearChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // logger.DebugFormat("year {0} -> {1}", e.OldValue, e.NewValue);

            var cdp = (ComboBoxDatePicker)d;

            //var coerced = CoerceYear(d, e.NewValue);
            //if (coerced != e.NewValue)
            //{
            //    cdp.Year = (int?)coerced;
            //}
            // else
            // смена года — было 29 февраля, меняем на 28
            cdp.FillDaysCombo();
            cdp.SetPickerDate();
        }

        private static object CoerceYear(DependencyObject d, object baseValue)
        {
            var cdp = (ComboBoxDatePicker)d;
            var newYear = (int?)baseValue;
            if (newYear.HasValue && newYear.Value < cdp.MinYear)
            {
                logger.DebugFormat("coerce year {0}", baseValue);
                // cdp.Year = cdp.MinYear;
                return cdp.MinYear;
            }
            return baseValue;
        }

        public int? Month
        {
            get { return (int?)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }

        public static readonly DependencyProperty MonthProperty =
            DependencyProperty.Register("Month", typeof(int?), typeof(ComboBoxDatePicker),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnMonthChanged)));

        private static void OnMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // logger.DebugFormat("month {0} -> {1}", e.OldValue, e.NewValue);

            var cdp = (ComboBoxDatePicker)d;
            // смена месяца — другой набор дней, если был день 31, меням на 30
            cdp.FillDaysCombo();
            cdp.SetPickerDate();
        }

        public int? Day
        {
            get { return (int?)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(int?), typeof(ComboBoxDatePicker),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnDayChanged)
                    ));

        private static void OnDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cdp = (ComboBoxDatePicker)d;

            cdp.SetPickerDate();
        }

        /// <summary>
        /// Глубина годов в списке, с сегодняшнего и ранее. По умолчанию 120.
        /// Если сегодня 2015 и YearsDepth == 5, то можно выбирать год 5 лет назад, то есть 2010.
        /// </summary>
        public int YearsDepth
        {
            get { return (int)GetValue(YearsDepthProperty); }
            set { SetValue(YearsDepthProperty, value); }
        }

        public static readonly DependencyProperty YearsDepthProperty =
            DependencyProperty.Register("YearsDepth", typeof(int), typeof(ComboBoxDatePicker),
                new FrameworkPropertyMetadata(120,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnYearsDepthChanged),
                    new CoerceValueCallback(CoerceYearsDepth)));

        private static void OnYearsDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cdp = (ComboBoxDatePicker)d;

            cdp.LoadYearsCombo();
        }

        private static object CoerceYearsDepth(DependencyObject d, object baseValue)
        {
            var newValue = (int)baseValue;
            if (newValue < 0)
            {
                return 0;
            }
            return baseValue;
        }

        /// <summary>
        /// Минимальный год для выбора.
        /// </summary>
        private int MinYear { get { return DateTime.Now.Year - YearsDepth; } }

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
            // logger.DebugFormat("load years");
            var years = Enumerable.Range(MinYear, YearsDepth + 1).Select(y => y.ToString()).ToList();
            years.Add("");
            years.Reverse();
            comboYears.ItemsSource = years;
        }

        private void LoadMonthsCombo()
        {
            var monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames.ToArray();
            var none = string.Empty;
            var names = new List<string> { none };
            names.AddRange(monthNames.Take(12));
            comboMonths.ItemsSource = names;
            comboMonths.SelectedValue = Month != null ? DateTimeFormatInfo.CurrentInfo.GetMonthName(Month.Value) : none;
        }

        private void LoadDaysCombo()
        {
            comboDays.ItemsSource = Days;
            FillDaysCombo();
        }

        private void FillDaysCombo()
        {
            // комбобокс остается, после DateOffset = null месяц == null, биндинг успевает сработать для дня
            var hrvm = (DataContext as HealthRecordViewModel);
            if (hrvm != null && hrvm.EventDate == null)
                return;

            var dpvm = (DataContext as DateOffsetViewModel);
            if (dpvm != null && dpvm.To == null)
                return;

            var days = GetDaysComboItems();
            var daysInMonth = days.Count() - 1;

            // в новом месяце меньше дней - выбриаем последний
            if (Day > daysInMonth)
            {
                Day = daysInMonth;
            }

            Days.SyncWith(days);
        }

        /// <summary>
        /// '', '1', ..., '30'
        /// </summary>
        /// <returns></returns>
        private string[] GetDaysComboItems()
        {
            int year = DateTime.Today.Year;
            if (Year.HasValue && Year.Value > 0 && Year.Value < 10000)
            {
                year = Year.Value;
            }

            var month = (Month != null && Month > 0 && Month < 13) ? Month.Value : DateTime.Today.Month;
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
        }

        private void comboMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void comboDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //logger.DebugFormat("day = {0}", Day);
        }

        private void comboYears_LostFocus(object sender, RoutedEventArgs e)
        {
            // исправляем после потери фокуса, чтобы можно было стереть написанное
            if (Year == null)
                comboYears.SelectedIndex = 0;
        }

        private void comboDays_LostFocus(object sender, RoutedEventArgs e)
        {
            // исправляем после потери фокуса, чтобы можно было стереть написанное
            if (Day == null)
                comboDays.SelectedIndex = 0;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (picker.SelectedDate.HasValue)
            {
                inSelectedDateChanged = true;
                Year = picker.SelectedDate.Value.Year;
                Month = picker.SelectedDate.Value.Month;
                Day = picker.SelectedDate.Value.Day;
                inSelectedDateChanged = false;
            }
        }

        private void SetPickerDate()
        {
            if (!inSelectedDateChanged)
                picker.SelectedDate = DateHelper.NullableDate(Year, Month, Day);
        }
    }
}