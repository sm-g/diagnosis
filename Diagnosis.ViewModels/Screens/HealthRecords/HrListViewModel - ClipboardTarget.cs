using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls.Autocomplete;
using Diagnosis.ViewModels.DataTransfer;
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

            HrData data = MakeHrData(hrs);

            var strings = string.Join(".\n", hrs.Select(hr => string.Join(", ", hr.GetOrderedCHIOs()))) + ".";

            IDataObject dataObj = new DataObject();
            dataObj.SetData(HrData.DataFormat.Name, data);
            dataObj.SetData(System.Windows.DataFormats.UnicodeText, strings);
            Clipboard.SetDataObject(dataObj, true);

            logger.LogHrs("copy", data.Hrs);
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
            else if (ido.GetDataPresent(TagData.DataFormat.Name))
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

        private static HrData MakeHrData(IEnumerable<HealthRecord> hrs)
        {
            var hrInfos = hrs.Select(hr => new HrData.HrInfo()
            {
                HolderId = (Guid)hr.Holder.Id,
                DoctorId = hr.Doctor.Id,
                CategoryId = hr.Category != null ? (Guid?)hr.Category.Id : null,
                From = new DateOffset(hr.FromDate),
                To = new DateOffset(hr.ToDate),
                Unit = hr.Unit,
                Chios = new List<ConfWithHio>(hr.GetOrderedCHIOs())
            }).ToList();

            return new HrData(hrInfos);
        }

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

            var pasted = Paste(hrData, index);

            SelectPasted(pasted);
            OnSaveNeeded(); // save all

            logger.LogHrs("paste", hrData.Hrs);
        }

        private void SelectPasted(IEnumerable<ShortHealthRecordViewModel> pasted)
        {
            SelectHealthRecords(pasted.Select(x => x.healthRecord));

            Contract.Assume(SelectedHealthRecord.healthRecord == pasted.Last().healthRecord);
            inManualFocusSetting = true;
            SelectedHealthRecord.IsFocused = true;
        }

        private IEnumerable<ShortHealthRecordViewModel> Paste(HrData hrData, int insertViewIndex)
        {
            var pasted = new List<HealthRecord>();
            var pastedVms = new List<ShortHealthRecordViewModel>();
            foreach (var hrInfo in hrData.Hrs)
            {
                if (hrInfo == null) continue;

                var newHr = holder.AddHealthRecord(AuthorityController.CurrentDoctor);
                // vm уже добавлена
                var newVm = HealthRecords.FirstOrDefault(vm => vm.healthRecord == newHr);
                Debug.Assert(newVm != null);

                FillHr(newHr, hrInfo);

                pastedVms.Add(newVm);
                pasted.Add(newHr);
            }

            hrManager.Reorder(pastedVms, view.Cast<ShortHealthRecordViewModel>().ToList(), insertViewIndex);
            return pastedVms;
        }

        private void FillHr(HealthRecord hr, DataTransfer.HrData.HrInfo hrInfo)
        {
            hrInfo.Chios.SyncAfterPaste(session);

            if (hrInfo.CategoryId != null)
            {
                using (var tr = session.BeginTransaction())
                {
                    hr.Category = session.Get<HrCategory>(hrInfo.CategoryId.Value);
                }
            }
            hr.FromDate.FillDateAndNowFrom(hrInfo.From);
            hr.ToDate.FillDateAndNowFrom(hrInfo.To);

            var unit = hrInfo.Unit;
            // если вставляем к пациенту без возраста
            if (hr.GetPatient().BirthYear == null && hrInfo.Unit == HealthRecordUnit.ByAge)
                unit = HealthRecordUnit.NotSet;

            hr.Unit = unit;
            hr.SetItems(hrInfo.Chios);
        }

        /// <summary>
        /// Add HrItems to selected HealthRecords or create new HealthRecords with them.
        /// </summary>
        /// <param name="data"></param>
        private void PasteTags(TagData data)
        {
            data.ItemObjects.SyncAfterPaste(session);

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