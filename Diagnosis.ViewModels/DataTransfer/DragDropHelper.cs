using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.DragDrop
{
    public static class DragDropHelper
    {
        public static bool FromSameCollection(this IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null || dropInfo.TargetCollection == null)
                return false;

            var sourceList = dropInfo.DragInfo.SourceCollection.ToList();
            var targetList = dropInfo.TargetCollection.ToList();
            return Equals(sourceList, targetList);
        }

        public static bool FromAutocomplete(this IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null)
                return false;

            var sourceList = dropInfo.DragInfo.SourceCollection.ToList();
            return sourceList is IEnumerable<TagViewModel>;
        }

        public static bool FromHrList(this IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null)
                return false;

            var sourceList = dropInfo.DragInfo.SourceCollection.ToList();
            return sourceList is IEnumerable<ShortHealthRecordViewModel>;
        }

        public static void StartDragWith(this IDragInfo dragInfo, IEnumerable<object> objects)
        {
            var itemCount = objects.Count();

            if (itemCount == 1)
            {
                dragInfo.Data = objects.First();
            }
            else if (itemCount > 1)
            {
                dragInfo.Data = TypeUtilities.CreateDynamicallyTypedList(objects);
            }

            dragInfo.Effects = (dragInfo.Data != null) ?
                                 DragDropEffects.Copy | DragDropEffects.Move :
                                 DragDropEffects.None;
        }
    }
}