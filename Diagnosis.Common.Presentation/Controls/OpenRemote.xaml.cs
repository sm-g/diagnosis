using Microsoft.Data.ConnectionUI;
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

namespace Diagnosis.Common.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for OpenRemote.xaml
    /// </summary>
    public partial class OpenRemote : UserControl
    {
        public OpenRemote()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var d = new DataConnectionDialog();
            DataSource.AddStandardDataSources(d);
            if (DataConnectionDialog.Show(d) == System.Windows.Forms.DialogResult.OK)
            {
                var vm = DataContext as dynamic;
                try
                {
                    vm.RemoteConnectionString = d.ConnectionString;
                    vm.RemoteProviderName = d.SelectedDataProvider.Name;
                }
                catch
                {


                }
            }

        }
    }
}
