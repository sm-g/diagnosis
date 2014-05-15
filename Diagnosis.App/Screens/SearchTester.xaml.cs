﻿using System;
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
using System.Windows.Shapes;

namespace Diagnosis.App.Screens
{
    /// <summary>
    /// Interaction logic for SearchTester.xaml
    /// </summary>
    public partial class SearchTester : UserControl
    {
        public SearchTester()
        {
            InitializeComponent();
            DataContext = new Diagnosis.App.ViewModels.SearchTester();
        }
    }
}
