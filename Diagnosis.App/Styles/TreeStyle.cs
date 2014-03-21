using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;



namespace Diagnosis.Styles
{
    partial class TreeStyle : ResourceDictionary
    {
        public TreeStyle()
        {
            InitializeComponent();
        }

        private void Bd_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var bd = sender as Border;
            try
            {
                Console.WriteLine("try Bd_SourceUpdated");
                bd.GetBindingExpression(Border.BackgroundProperty).UpdateTarget();
            }
            catch
            {
                // IsSelected trigger sets Background
                Console.WriteLine("exc Bd_SourceUpdated");
            }
        }
        private void Bd_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var bd = sender as Border;
            try
            {
                Console.WriteLine("try Bd_TargetUpdated");
                bd.GetBindingExpression(Border.BackgroundProperty).UpdateTarget();
            }
            catch
            {
                // IsSelected trigger sets Background
                Console.WriteLine("exc Bd_TargetUpdated");
            }
        }
    }
}
