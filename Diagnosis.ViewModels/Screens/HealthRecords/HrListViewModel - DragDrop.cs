using Diagnosis.Models;
using Diagnosis.ViewModels.Controls.Autocomplete;
using Diagnosis.ViewModels.DragDrop;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
            /// Drag Hrs (to autocomplete or hrlist)
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
                return dragInfo.SourceItems.Cast<ShortHealthRecordViewModel>().Any();
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
                if (dropInfo.DragInfo == null || dropInfo.DragInfo.SourceCollection == null || !data.Any())
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
                else if (dropInfo.FromSameCollection())
                {
                    var vms = ExtractData(dropInfo.Data).Cast<ShortHealthRecordViewModel>();
                    if (master.CanDropTo(vms, dropInfo.TargetGroup))
                    {
                        dropInfo.Effects = DragDropEffects.Move;
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        if (dropInfo.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                            dropInfo.Effects |= DragDropEffects.Copy;
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
                    //drop hrs from hrslist
                    var group = dropInfo.TargetGroup != null ? dropInfo.TargetGroup.Name : null;
                    var hrs = data.Cast<ShortHealthRecordViewModel>();
                    if (dropInfo.KeyStates == DragDropKeyStates.ControlKey)
                    {
                        CopyHrsToMaster(hrs, group, insertView);
                    }
                    else
                    {
                        ReorderHrsInMaster(hrs, group, insertView);
                    }
                }
                else if (dropInfo.FromAutocomplete())
                {
                    //drop tags from autocomplete
                    var tags = data.Cast<TagViewModel>();
                    AddHrWithTagsToMaster(tags);
                }
                master.SaveHrs();
                //logger.DebugFormat("selected after save {0} ", master.SelectedHealthRecords.Count());
            }

            private void CopyHrsToMaster(IEnumerable<ShortHealthRecordViewModel> hrs, object group, int insertView)
            {
                var hrData = MakeHrData(hrs.Select(x => x.healthRecord));
                var pastedVms = master.Paste(hrData, insertView);
                foreach (var item in pastedVms)
                {
                    master.SetGroupObject(item, group);
                }
                master.SelectPasted(pastedVms);
            }

            private void ReorderHrsInMaster(IEnumerable<ShortHealthRecordViewModel> hrs, object group, int insertView)
            {
                //  logger.DebugFormat("selected bef {0} ", master.SelectedHealthRecords.Count());
                master.Reorder(hrs, insertView, group);
                //  logger.DebugFormat("selected after dd {0} ", master.SelectedHealthRecords.Count());
            }

            private void AddHrWithTagsToMaster(IEnumerable<TagViewModel> tags)
            {
                var newHR = master.holder.AddHealthRecord(master.AuthorityController.CurrentDoctor);
                var items = tags.Select(t => new ConfWithHio(t.Blank, t.Confidence)).ToList();
                newHR.SetItems(items);
            }
        }
    }
}