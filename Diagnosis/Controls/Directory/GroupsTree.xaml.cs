
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            tree.DataContext = Diagnosis.DataCreator.Symptoms;
        }
    }
}
