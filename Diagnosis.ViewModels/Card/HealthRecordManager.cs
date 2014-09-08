using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public class HealthRecordManager
    {
        private readonly Appointment app;
        private ObservableCollection<HealthRecordViewModel> _healthRecords;

        public event EventHandler HealthRecordsLoaded;

        public event PropertyChangedEventHandler HrVmPropertyChanged;

        public ObservableCollection<HealthRecordViewModel> HealthRecords
        {
            get
            {
                if (_healthRecords == null)
                {
                    IList<HealthRecordViewModel> hrVMs;
                    using (var tester = new PerformanceTester((ts) => Debug.Print("making healthrecords for {0}: {1}", app, ts)))
                    {
                        hrVMs = app.HealthRecords.Select(hr => CreateViewModel(hr)).ToList();
                    }
                    _healthRecords = new ObservableCollection<HealthRecordViewModel>(hrVMs);

                    OnHealthRecordsLoaded();
                }
                return _healthRecords;
            }
        }

        private HealthRecordViewModel CreateViewModel(HealthRecord hr)
        {
            hr.PropertyChanged += hr_PropertyChanged;
            var vm = new HealthRecordViewModel(hr);
            vm.PropertyChanged += (s, e) => { OnHrVmPropertyChanged(e); };
            return vm;
        }

        public ObservableCollection<HealthRecordViewModel> DeletedHealthRecords { get; private set; }

        public HealthRecordManager(Appointment app)
        {
            this.app = app;
            app.HealthRecordsChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (HealthRecord item in e.NewItems)
                    {
                        var hrVM = CreateViewModel(item);
                        HealthRecords.Add(hrVM);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (HealthRecord item in e.OldItems)
                    {
                        var hrVM = HealthRecords.Where(vm => vm.healthRecord == item).FirstOrDefault();
                        HealthRecords.Remove(hrVM);
                    }
                }
            };

            DeletedHealthRecords = new ObservableCollection<HealthRecordViewModel>();
        }

        public void DeleteCheckedHealthRecords()
        {
            HealthRecords.Where(hr => hr.IsChecked).ToList().ForAll(hr =>
            {
                hr.healthRecord.IsDeleted = true;
            });
        }


        protected virtual void OnHealthRecordsLoaded()
        {
            var h = HealthRecordsLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected virtual void OnHrVmPropertyChanged(PropertyChangedEventArgs e)
        {
            var h = HrVmPropertyChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        private void hr_Deleted(object sender, EditableEventArgs e)
        {
            var hr = e.entity as HealthRecord;
            app.RemoveHealthRecord(hr);

            var hrVM = HealthRecords.Where(vm => vm.healthRecord == hr).FirstOrDefault();
            HealthRecords.Remove(hrVM);
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var hr = sender as HealthRecord;
            if (e.PropertyName == "IsDeleted")
            {
                if (hr.IsDeleted)
                {
                    var vm = HealthRecords.Where(x => x.healthRecord == hr).First();
                    DeletedHealthRecords.Add(vm);
                    HealthRecords.Remove(vm);
                }
                else
                {
                    var vm = DeletedHealthRecords.Where(x => x.healthRecord == hr).First();
                    DeletedHealthRecords.Remove(vm);
                    HealthRecords.Add(vm);
                }
            }
        }
    }
}