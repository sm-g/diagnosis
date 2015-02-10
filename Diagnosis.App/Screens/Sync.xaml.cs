using Diagnosis.ViewModels.Screens;
using Microsoft.Data.ConnectionUI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Diagnosis.App.Screens
{
    public partial class Sync : UserControl
    {
        public Sync()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                log.ScrollToEnd();

                log.TextChanged += (s1, e1) =>
                {
                    Action action = () =>
                    {
                        log.ScrollToEnd();
                    };
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
                };
            };

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var d = new DataConnectionDialog();
            DataSource.AddStandardDataSources(d);
            if (DataConnectionDialog.Show(d) == System.Windows.Forms.DialogResult.OK)
            {
                var vm = DataContext as SyncViewModel;
                if (vm != null)
                {
                    connectionString.Text = d.ConnectionString;
                    vm.ProviderName = d.SelectedDataProvider.Name;
                }
            }

        }
    }
}