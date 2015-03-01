using Diagnosis.Common;
using Diagnosis.ViewModels.Screens;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Diagnosis.Client.App.Windows
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private bool inNavigated;

        public HelpWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var uri = MakeUri(Vm.Topic);
                webBrowser.Navigate(uri);

                if (Left < 0)
                    Left = 0;

                Vm.PropertyChanged += Vm_PropertyChanged;
            };
            Unloaded += (s, e) =>
            {
                Vm.PropertyChanged -= Vm_PropertyChanged;
            };
            webBrowser.Navigated += (s, e) =>
            {
                inNavigated = true;

                var lastSlashIndex = e.Uri.AbsolutePath.LastIndexOf('/');
                var page = e.Uri.AbsolutePath.Substring(lastSlashIndex + 1);
                var anchorIndex = page.LastIndexOf('#');
                var anchor = e.Uri.AbsolutePath.Substring(anchorIndex + 1);

                Vm.Topic = anchorIndex >= 0 ? anchor : page;

                inNavigated = false;
            };
        }

        private HelpViewModel Vm
        {
            get
            {
                HelpViewModel vm = null;
                Dispatcher.Invoke((Action)(() =>
                {
                    vm = DataContext as HelpViewModel;
                }));
                return vm;
            }
        }

        private static Uri MakeUri(string topic)
        {
            var helpPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Help\\");

            if (topic.IsNullOrEmpty())
                return new Uri(string.Format("{0}index.html", helpPath));
            if (topic.Contains("key"))

                return new Uri( string.Format("{0}hotkeys.html#{1}", helpPath, topic));

            return new Uri(string.Format("{0}index.html#{1}", helpPath, topic));
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Topic" && !inNavigated)
            {
                var uri = MakeUri(Vm.Topic);
                Dispatcher.Invoke((Action)(() =>
                    webBrowser.Navigate(uri)));
            }
        }
    }
}