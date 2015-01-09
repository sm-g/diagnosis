using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace Diagnosis.Common
{
    public static class AvalonExtensions
    {
        public static IEnumerable<LayoutAnchorable> LayoutAnchorables(this ILayoutElement element)
        {
            return element.Descendents()
                .OfType<LayoutAnchorable>();
        }
        public static IEnumerable<LayoutAnchorablePane> LayoutAnchorablePanes(this ILayoutElement element)
        {
            return element.Descendents()
                .OfType<LayoutAnchorablePane>();
        }

        public static IEnumerable<LayoutAnchorable> LayoutAnchorables(this ILayoutElement element, object content)
        {
            return element.Descendents()
                .OfType<LayoutAnchorable>()
                .Where(x => x.Content == content);
        }
        public static IEnumerable<LayoutAnchorable> LayoutAnchorables(this ILayoutElement element, string contentId)
        {
            return element.Descendents()
                .OfType<LayoutAnchorable>()
                .Where(x => x.ContentId == contentId);
        }
        public static IEnumerable<LayoutAnchorablePane> LayoutAnchorablePanes(this ILayoutElement element, string name)
        {
            return element.Descendents()
                .OfType<LayoutAnchorablePane>()
                .Where(x => x.Name == name);
        }
    }
}
