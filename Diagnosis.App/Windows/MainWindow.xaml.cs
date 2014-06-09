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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EventAggregator;
using Diagnosis.App.ViewModels;
using Diagnosis.App.Messaging;

namespace Diagnosis.App.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Subscribe((int)EventID.OpenSettings, (e) =>
            {
                var settingsVM = e.GetValue<SettingsViewModel>(Messages.Settings);
                var settingsDialog = new SettingsWindow();
                settingsDialog.Owner = this;
                settingsDialog.DataContext = settingsVM;
                var result = settingsDialog.ShowDialog();
            });

            DataContext = new MainWindowViewModel(frame.NavigationService);
        }
    }
}
