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

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for PropertiesChooser.xaml
    /// </summary>
    public partial class PropertiesChooser : UserControl
    {
        public PropertiesChooser()
        {
            InitializeComponent();

            DataContext = DataCreator.Properties;
        }
    }
}
