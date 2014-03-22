using Diagnosis.Data.Repositories;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DoctorsManager : ViewModelBase
    {
        private DoctorViewModel _current;
        IDoctorRepository doctorRepo;

        public ObservableCollection<DoctorViewModel> Doctors { get; private set; }

        public DoctorViewModel CurrentDoctor
        {
            get
            {
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;

                    OnPropertyChanged(() => CurrentDoctor);
                }
            }
        }

        public DoctorsManager(IDoctorRepository docRepo)
        {
            Contract.Requires(docRepo != null);
            this.doctorRepo = docRepo;
            var doctorVMs = doctorRepo.GetAll().Select(d => new DoctorViewModel(d)).ToList();
            foreach (var dvm in doctorVMs)
            {
                dvm.Committed += d_Committed;
            }

            Doctors = new ObservableCollection<DoctorViewModel>(doctorVMs);
            if (Doctors.Count > 0)
            {
                CurrentDoctor = Doctors[0];
            }
        }

        private void d_Committed(object sender, EventArgs e)
        {
            var doctorVM = (sender as DoctorViewModel);
            if (doctorVM != null)
            {
                doctorRepo.SaveOrUpdate(doctorVM.doctor);
            }
        }
    }
}