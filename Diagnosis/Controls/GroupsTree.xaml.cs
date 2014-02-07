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
    /// Interaction logic for GroupsTree.xaml
    /// </summary>
    public partial class GroupsTree : UserControl
    {
        public GroupsTree()
        {
            InitializeComponent();
        }
        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Diagnosis.ViewModels.SymptomViewModel.CreateSymptoms();

        }
    }
}
