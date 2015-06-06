using Diagnosis.Common;
using Diagnosis.ViewModels.Screens;
using System;
using System.Linq;
using Xceed.Wpf.AvalonDock.Layout;

namespace Diagnosis.Client.App.Windows.Shell
{
    internal class LayoutInitializer : ILayoutUpdateStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(LayoutInitializer));

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            // AD wants to add the anchorable into destinationContainer

            var content = anchorableToShow.Content as PaneViewModel;
            logger.DebugFormat("before insert {0} to {1}",
                anchorableToShow.Content,
                destinationContainer == null ? "''" :
                    string.Format("{0} with {1} childs", destinationContainer.GetType().Name, destinationContainer.ChildrenCount));

            string id = content != null ? content.ContentId : anchorableToShow.ContentId;

            var panes = layout.LayoutAnchorablePanes();
            var anchs = layout.LayoutAnchorables();
            var pane = panes.FirstOrDefault(d => d.Name == id);
            if (pane != null)
            {
                if (content != null)
                {
                    // есть VM, повторно показываем панель

                    //logger.DebugFormat("SetIsAutoHiddenSettingCallback");
                    content.SetIsAutoHiddenSettingCallback((willBeAutoHidden) =>
                    {
                        logger.DebugFormat("AutoHiddenChangingCallback. IsAutoHidden {0}, IsHidden {1}, willBeAutoHidden {1}",
                            anchorableToShow.IsAutoHidden, anchorableToShow.IsHidden, willBeAutoHidden);
                        if (willBeAutoHidden != anchorableToShow.IsAutoHidden)
                            anchorableToShow.ToggleAutoHide();
                    });
                }
                pane.Children.Add(anchorableToShow);
                return true;
            }
            return false;
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }
    }
}