﻿using Diagnosis.ViewModels.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;
using Diagnosis.Common;

namespace Diagnosis.App.Windows.Shell
{
    class LayoutInitializer : ILayoutUpdateStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(LayoutInitializer));
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            // AD wants to add the anchorable into destinationContainer            
            logger.DebugFormat("before insert {0} to {1}",
                anchorableToShow.ContentId, destinationContainer == null ? "''" :
                    string.Format("container with {0} childs", destinationContainer.ChildrenCount));
            var content = anchorableToShow.Content as PaneViewModel;
            string id = null;
            if (content != null)
                id = content.ContentId;
            else
                id = anchorableToShow.ContentId;

            var panes = layout.LayoutAnchorablePanes();
            var ahcs = layout.LayoutAnchorables();
            var pane = panes.FirstOrDefault(d => d.Name == id);
            if (pane != null)
            {
                content.IsAutoHiddenChanging += (s, e) => // leak subscription
                {
                    if (e.value != anchorableToShow.IsAutoHidden)
                        anchorableToShow.ToggleAutoHide();
                };
                pane.Children.Add(anchorableToShow);
                return true;
            }
            return false;

        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
            if (anchorableShown.Content != null)
            {
                var pane = anchorableShown.Content as PaneViewModel;
                if (pane != null && pane.HideAfterInsert && !anchorableShown.IsAutoHidden)
                {
                    anchorableShown.ToggleAutoHide();
                }
            }
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
