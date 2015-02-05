using Diagnosis.ViewModels.Screens;
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

namespace Diagnosis.ServerApp.Screens
{
    /// <summary>
    /// Interaction logic for Specialities.xaml
    /// </summary>
    public partial class Specialities : UserControl
    {
        public Specialities()
        {
            InitializeComponent();
            DataContext = new SpecialityListViewModel();
        }
    }
}
