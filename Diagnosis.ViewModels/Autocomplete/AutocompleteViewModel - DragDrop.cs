using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Diagnosis.ViewModels.DragDrop;

namespace Diagnosis.ViewModels.Autocomplete
{
    public partial class AutocompleteViewModel : IDroppable, IDraggable
    {
        private bool inDragDrop;

        private bool _dropTarget;

        private bool _dragSource;

        public bool InDragDrop
        {
            get { return inDragDrop; }
            private set { inDragDrop = value; }
        }

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
            /// Data is tags without Last tag.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !t.IsLast);
                dragInfo.StartDragWith(tags);
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !t.IsLast);
                return tags.Count() > 0;
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
            private readonly AutocompleteViewModel master;

            public DropTargetHandler(AutocompleteViewModel master)
            {
                this.master = master;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                if (dropInfo.FromAutocomplete())
                {
                    if (dropInfo.FromSameCollection())
                        dropInfo.Effects = DragDropEffects.Move;
                    else
                        dropInfo.Effects = DragDropEffects.Copy;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (data.Count() == 0)
                    return;

                logger.DebugFormat("ddrop {0} {1}", data.Count(), data.First().GetType());

                var insertIndex = dropInfo.InsertIndex;

                if (dropInfo.FromAutocomplete())
                {
                    var tags = data.Cast<TagViewModel>();
                    if (dropInfo.FromSameCollection())
                    {
                        // reorder tags
                        master.InDragDrop = true;
                        foreach (var tag in tags)
                        {
                            var old = master.Tags.IndexOf(tag);
                            //master.Tags.RemoveAt(old);
                            //if (old < insertIndex)
                            //{
                            //    --insertIndex;
                            //}
                            var n = old < insertIndex ? insertIndex - 1 : insertIndex;

                            // not after last
                            if (n == master.Tags.IndexOf(master.LastTag))
                                n--;

                            n = Math.Max(n, 0); // when single n == -1

                            if (old != n) // prevent deselecting
                                master.Tags.Move(old, n);
                        }
                        //foreach (var tag in tags)
                        //{
                        //    master.Tags.Insert(insertIndex, tag);
                        //}
                        master.InDragDrop = false;
                    }
                    else
                    {
                        // copy tags' HrItemObjects or query

                        foreach (var tag in tags)
                        {
                            if (tag.BlankType == BlankType.None)
                            {
                                master.AddTag(tag.Query, insertIndex).Validate(master.Validator);
                            }
                            else
                            {
                                var item = master.recognizer.EntityOf(tag);
                                master.AddTag(item, insertIndex).Validate(master.Validator);
                            }
                        }
                    }
                    master.LastTag.IsSelected = false;
                }


            }
        }
    }
}