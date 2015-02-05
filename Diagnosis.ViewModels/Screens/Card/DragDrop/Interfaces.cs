using System;
using System.Collections.Generic;
using GongSolutions.Wpf.DragDrop;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.DragDrop
{
    public interface IDroppable
    {
        IDropTarget DropHandler { get; }
        bool IsDropTargetEnabled { get; set; }

    }
    public interface IDraggable
    {
        IDragSource DragHandler { get; }
        bool IsDragSourceEnabled { get; set; }

    }
}
