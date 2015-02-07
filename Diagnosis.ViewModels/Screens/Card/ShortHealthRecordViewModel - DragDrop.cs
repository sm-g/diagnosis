﻿using Diagnosis.Common;
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
    public partial class ShortHealthRecordViewModel : IDroppable
    {
        private bool _dropTarget;

        public IDropTarget DropHandler { get; private set; }

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

        public class DropTargetHandler : DefaultDropHandler
        {
            private readonly ShortHealthRecordViewModel master;

            public DropTargetHandler(ShortHealthRecordViewModel master)
            {
                this.master = master;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                if (dropInfo.FromAutocomplete())
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.Scroll;
                }
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var data = ExtractData(dropInfo.Data).Cast<object>();
                if (data.Count() == 0)
                    return;

                logger.DebugFormat("drop to hr {0} {1}", data.Count(), data.First().GetType());

                if (dropInfo.FromAutocomplete())
                {
                    // drop tags from autocomplete
                    // add tags to hr
                    var tags = data.Cast<TagViewModel>();

                    var hr = master.healthRecord;
                    var items = tags.Select(t => t.Entity).ToList();
                    hr.AddItems(items);
                }
            }
        }
    }
}