﻿using System.Windows.Controls;

namespace Diagnosis.App.Controls.CardParts
{
    /// <summary>
    /// Interaction logic for HealthRecord.xaml
    /// </summary>
    public partial class HealthRecord : UserControl
    {
        public HealthRecord()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
#if !DEBUG
                order.Visibility = System.Windows.Visibility.Collapsed;
#endif
            };
        }
    }
}