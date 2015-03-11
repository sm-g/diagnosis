using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public partial class HrListViewModel : IClipboardTarget
    {
        /// <summary>
        /// When set focus from VM, do not focus
        /// </summary>
        public bool inManualFocusSetting;

        private Action<HealthRecord, HrData.HrInfo> fillHr;
        private Action<IList<IHrItemObject>> syncHios;

        private string[] acceptFormats = new[] {
            HrData.DataFormat.Name,
            TagData.DataFormat.Name
        };

        #region IClipboardTarget

        public void Cut()
        {
            logger.Debug("cut");
            Copy();
            hrManager.DeleteCheckedHealthRecords(withCancel: false);
        }

        public void Copy()
        {
            var hrs = hrManager.GetSelectedHrs();
            var hrInfos = hrs.Select(hr => new HrData.HrInfo()
            {
                HolderId = (Guid)hr.Holder.Id,
                DoctorId = hr.Doctor.Id,
                CategoryId = hr.Category != null ? (Guid?)hr.Category.Id : null,
                FromDay = hr.FromDay,
                FromMonth = hr.FromMonth,
                FromYear = hr.FromYear,
                Unit = hr.Unit,
                Hios = new List<IHrItemObject>(hr.HrItems.Select(x => x.Entity))
            }).ToList();

            var data = new HrData(hrInfos);

            var strings = string.Join(".\n", hrs.Select(hr => string.Join(", ", hr.GetOrderedEntities()))) + ".";

            IDataObject dataObj = new DataObject();
            dataObj.SetData(HrData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, false);

            logger.LogHrs("copy", hrInfos);
        }

        public void Paste()
        {
            Contract.Ensures(HealthRecords.Count >= Contract.OldValue(HealthRecords).Count);

            var ido = Clipboard.GetDataObject();

            if (ido.GetDataPresent(HrData.DataFormat.Name))
            {
                var hrData = (HrData)ido.GetData(HrData.DataFormat.Name);
                PasteHrs(hrData);
            }
            else
                if (ido.GetDataPresent(TagData.DataFormat.Name))
                {
                    var hiosData = (TagData)ido.GetData(TagData.DataFormat.Name);
                    PasteTags(hiosData);
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
        /// Add HealthRecords before SelectedHealthRecords or to the end of list.
        /// </summary>
        /// <param name="hrData"></param>
        private void PasteHrs(HrData hrData)
        {
            int index;
            if (SelectedHealthRecord == null)
                index = view.Count;
            else
                index = view.IndexOf(SelectedHealthRecord);

            var pasted = new List<HealthRecord>();
            var pastedVms = new List<ShortHealthRecordViewModel>();
            foreach (var hr2 in hrData.Hrs)
            {
                if (hr2 == null) continue;

                var newHr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                // vm уже добавлена
                var newVm = HealthRecords.FirstOrDefault(vm => vm.healthRecord == newHr);
                Debug.Assert(newVm != null);
                fillHr(newHr, hr2);
                // теперь запись заполнена
                pastedVms.Add(newVm);
                pasted.Add(newHr);
            }

            hrManager.Reorder(pastedVms, HealthRecordsView, index);

            SelectHealthRecords(pasted);
            OnSaveNeeded(); // save all

            Contract.Assume(SelectedHealthRecord.healthRecord == pasted.Last());
            inManualFocusSetting = true;
            SelectedHealthRecord.IsFocused = true;
            //inManualFocusSettng = false;

            logger.LogHrs("paste", hrData.Hrs);
        }
        /// <summary>
        /// Add HrItems to selected HealthRecords or create new HealthRecords with them.
        /// </summary>
        /// <param name="data"></param>
        private void PasteTags(TagData data)
        {
            // for new word case
            syncHios(data.ItemObjects);

            var hrs = hrManager.GetSelectedHrs();
            if (hrs.Count > 0)
            {
                // add hios to end of Selected Hrs
                hrs.ForAll(hr => hr.AddItems(data.ItemObjects));
                OnSaveNeeded(hrManager.GetSelectedHrs());
            }
            else
            {
                // new hr with pasted hios
                var newHR = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                newHR.AddItems(data.ItemObjects);
                OnSaveNeeded(); // save all
            }
            logger.LogHrItemObjects("paste", data.ItemObjects);
        }


    }
}