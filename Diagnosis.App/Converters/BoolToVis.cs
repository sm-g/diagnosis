﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Diagnosis.App.Converters
{
    /// <summary>
    /// If Visibility.Hidden passed in parameters, use it for false value.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                bool val = (bool)value;
                if (val)
                    return Visibility.Visible;
                else
                    if (parameter != null && parameter is Visibility)
                        return parameter;
                    else
                        return Visibility.Collapsed;
            }
            if (value == null)
            {
                if (parameter != null && parameter is Visibility)
                    return parameter;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && targetType == typeof(bool))
            {
                Visibility val = (Visibility)value;
                if (val == Visibility.Visible)
                    return true;
                else
                    return false;
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool");
        }
    }

    public class NegBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                bool val = !(bool)value;
                if (val)
                    return Visibility.Visible;
                else
                    if (parameter != null && parameter is Visibility)
                        return parameter;
                    else
                        return Visibility.Collapsed;
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: bool and return type: Visibility");
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && targetType == typeof(bool))
            {
                Visibility val = (Visibility)value;
                if (val == Visibility.Visible)
                    return false;
                else
                    return true;
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool");
        }
    }
}