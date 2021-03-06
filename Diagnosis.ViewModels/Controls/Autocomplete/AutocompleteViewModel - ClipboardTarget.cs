﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.DataTransfer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Controls.Autocomplete
{
    public partial class AutocompleteViewModel : IClipboardTarget
    {
        private string[] acceptFormats = new[] {
            //HrData.DataFormat.Name,
            TagData.DataFormat.Name
        };

        #region IClipboardTarget

        public void Cut()
        {
            Contract.Ensures(Tags.Count <= Contract.OldValue(Tags).Count);

            logger.Debug("cut");
            Copy();

            // do not remove init tags
            var completed = SelectedTags.Where(t => t.State == State.Completed);
            completed.ForAll(t => t.DeleteCommand.Execute(null));
        }

        public void Copy()
        {
            var hios = GetCHIOsOfSelectedCompleted().ToList();
            var data = new TagData(hios);

            var strings = string.Join(", ", hios);

            IDataObject dataObj = new DataObject();

            dataObj.SetData(TagData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, true);

            logger.LogHrItemObjects("copy", hios);
        }

        public void Paste()
        {
            var ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(TagData.DataFormat.Name))
            {
                var data = (TagData)ido.GetData(TagData.DataFormat.Name);
                PasteTags(data);
            }
        }

        public bool CanPaste()
        {
            var ido = Clipboard.GetDataObject();
            var result = ido.GetFormats().Any(x => acceptFormats.Contains(x));
            return result;
        }

        #endregion IClipboardTarget

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void PasteTags(TagData data)
        {
            Contract.Ensures(Tags.Count >= Contract.OldValue(Tags).Count);

            // paste before first SelectedTag
            var index = Tags.IndexOf(SelectedTag);
            SelectedTags.ForEach(t => t.IsSelected = false);

            data.ItemObjects.SyncAfterPaste(sugMaker.Session);

            foreach (var item in data.ItemObjects)
            {
                if (item == null) continue;

                var tag = AddTag(item, index++);
                tag.IsSelected = true;

                tag.SetSignalization();
            }
            logger.LogHrItemObjects("paste", data.ItemObjects);
        }


    }
}