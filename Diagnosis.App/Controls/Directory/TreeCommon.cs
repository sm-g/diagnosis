using Diagnosis.App;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
namespace Diagnosis.App.Controls
{
    public class TreeCommon : UserControl
    {
        [Flags]
        public enum TreeItemType
        {
            Light = 0,
            WithSearch = 1,
            WithEditor = 2,
            Full = 3
        }

        public TreeItemType ItemType
        {
            get { return (TreeItemType)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(TreeItemType), typeof(FullTree));

    }
}
