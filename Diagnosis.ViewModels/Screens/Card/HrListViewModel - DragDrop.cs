using Diagnosis.ViewModels.Autocomplete;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Diagnosis.ViewModels.DragDrop;
namespace Diagnosis.ViewModels.Screens
{
    public partial class HrListViewModel : IDraggable, IDroppable
    {
        private bool _dragSource;
        private bool _dropTarget;

        public IDropTarget DropHandler { get; private set; }

        public IDragSource DragHandler { get; private set; }

        public bool IsDragSourceEnabled
        {
            get
            {
                return _dragSource;
            }
            set
            {
                if (_dragSource != value)
                {
                    _dragSource = value;
                    OnPropertyChanged(() => IsDragSourceEnabled);
                }
            }
        }

        public bool IsDropTargetEnabled
        {
            get
            {
                return _dropTarget;
            }
            set
            {
                if (_dropTarget != value)
                {
                    _dropTarget = value;
                    OnPropertyChanged(() => IsDropTargetEnabled);
                }
            }
        }

        public class DragSourceHandler : IDragSource
        {
            /// <summary>
            /// Data is hrs vm.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var hrs = dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>();
                dragInfo.StartDragWith(hrs);
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                return dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>().Count() > 0;
            }

            public void DragCancelled()
            {
            }

            public void Dropped(IDropInfo dropInfo)
            {
            }
        }

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly HrListViewModel master;

            public DropTargetHandler(HrListViewModel master)
            {
                this.master = master;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null || data.Count() == 0)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
                else if (dropInfo.FromSameCollection())
                {
                    var vms = ExtractData(dropInfo.Data).Cast<ShortHealthRecordViewModel>();
                    if (master.CanMove(vms, dropInfo.TargetGroup))
                    {
                        dropInfo.Effects = DragDropEffects.Move;
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    }
                }
                else if (dropInfo.FromAutocomplete())
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();

                //  logger.DebugFormat("ddrop {0} {1}", data.Count(), data.First().GetType());

                var insertView = dropInfo.InsertIndex;
                if (dropInfo.FromSameCollection())
                {
                    // drop hrs from hrslist

                    //  logger.DebugFormat("selected bef {0} ", master.SelectedHealthRecords.Count());

                    var group = dropInfo.TargetGroup != null ? dropInfo.TargetGroup.Name : null;
                    master.Reorder(data, insertView, group);

                    //  logger.DebugFormat("selected after dd {0} ", master.SelectedHealthRecords.Count());
                }
                else if (dropInfo.FromAutocomplete())
                {
                    // drop tags from autocomplete

                    var tags = data.Cast<TagViewModel>();

                    // new hr from tags
                    var newHR = master.AddHr();
                    var items = tags.Select(t => t.Entity).ToList();
                    newHR.SetItems(items);
                }
                master.OnSaveNeeded();
                //logger.DebugFormat("selected after save {0} ", master.SelectedHealthRecords.Count());
            }
        }
    }
}