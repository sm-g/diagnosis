using Diagnosis.ViewModels.DragDrop;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Controls.Autocomplete
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
        public void OnDrop(DragEventArgs e)
        {
            logger.DebugFormat("drop {0}", e.Data.ToString());

            string text = GetDroppedText(e);

            if (text != null)
            {
                // drop strings - make tag with query and complete it

                var strings = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strings)
                {
                    var tag = CreateTag(str);
                    var sugg = sugMaker.SearchForSuggesstions(str, null).FirstOrDefault();
                    CompleteCommon(tag, sugg, true);
                    AddTag(tag);
                }
            }
        }

        private static string GetDroppedText(DragEventArgs e)
        {
            string text = null;
            string unicodeText = null;

            // prefer unicode format
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                text = (string)e.Data.GetData(DataFormats.Text);
            }
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                unicodeText = (string)e.Data.GetData(DataFormats.UnicodeText);
            }
            if (unicodeText != null)
                text = unicodeText;
            return text;
        }

        public class DragSourceHandler : IDragSource
        {
            private readonly AutocompleteViewModel master;

            public DragSourceHandler(AutocompleteViewModel master)
            {
                this.master = master;
            }

            /// <summary>
            /// Data is tags without empty Last tag.
            /// </summary>
            /// <param name="dragInfo"></param>
            public void StartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !(t.State == State.Init && t.IsLast));
                master.CompleteTypings();

                dragInfo.StartDragWith(tags);
            }

            public bool CanStartDrag(IDragInfo dragInfo)
            {
                var tags = dragInfo.SourceItems.Cast<TagViewModel>().Where(t => !(t.State == State.Init && t.IsLast));
                return tags.Any();
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
                if (!data.Any())
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
                                master.tagsWritable.Move(old, n);
                        }
                        //foreach (var tag in tags)
                        //{
                        //    master.Tags.Insert(insertIndex, tag);
                        //}
                        master.InDragDrop = false;
                    }
                    else
                    {
                        // copy tags' content
                        foreach (var tag in tags)
                        {
                            Contract.Assume(tag.State == State.Completed);
                            master.AddTag(tag.ToChio(), insertIndex).SetSignalization();
                        }
                    }
                    master.LastTag.IsSelected = false;
                }
            }
        }
    }
}