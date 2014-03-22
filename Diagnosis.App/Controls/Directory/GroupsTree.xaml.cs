using Diagnosis.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.App.Controls
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
            tree.DataContext = DataCreator.Symptoms;
        }
    }
}
