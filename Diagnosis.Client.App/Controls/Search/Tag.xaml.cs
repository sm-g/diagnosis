﻿using System.Windows.Controls;

namespace Diagnosis.Client.App.Controls.Search
{
    /// <summary>
    /// Interaction logic for Tag.xaml
    /// </summary>
    public partial class Tag : UserControl
    {
        public Tag()
        {
            InitializeComponent();
            query.TargetUpdated += (s, e) =>
            {
                // текст поменялся из кода
                query.CaretIndex = query.Text.Length;
            };
        }
    }
}