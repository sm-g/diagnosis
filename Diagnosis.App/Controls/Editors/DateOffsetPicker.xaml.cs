﻿using Diagnosis.Common;
using System.Windows;
using System.Windows.Controls;

namespace Diagnosis.App.Controls.Editors
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

        public bool ReadOnlyOffset
        {
            get { return (bool)GetValue(ReadOnlyOffsetProperty); }
            set { SetValue(ReadOnlyOffsetProperty, value); }
        }

        public static readonly DependencyProperty ReadOnlyOffsetProperty =
            DependencyProperty.Register("ReadOnlyOffset", typeof(bool), typeof(DateOffsetPicker), new PropertyMetadata(false));

        public DateOffsetPicker()
        {
            InitializeComponent();
        }
    }
}