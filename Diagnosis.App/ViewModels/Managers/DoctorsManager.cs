using Diagnosis.App.Messaging;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DoctorsManager : ViewModelBase
    {
        private DoctorViewModel _current;
        private IDoctorRepository repository;

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

                    OnPropertyChanged("CurrentDoctor");
                    this.Send((int)EventID.CurrentDoctorChanged, new DoctorModelParams(_current.doctor).Params);
                }
            }
        }

        public DoctorViewModel GetByModel(Doctor doctor)
        {
            return Doctors.FirstOrDefault(a => a.doctor == doctor);
        }

        public DoctorsManager(IDoctorRepository repo)
        {
            Contract.Requires(repo != null);
            this.repository = repo;

            var doctorVMs = repository.GetAll().Select(d => new DoctorViewModel(d)).ToList();
            doctorVMs.ForEach(dvm => SubscribeDoctor(dvm));
            Doctors = new ObservableCollection<DoctorViewModel>(doctorVMs);

            if (Doctors.Count > 0)
            {
                CurrentDoctor = Doctors[0];
            }
        }

        private void SubscribeDoctor(DoctorViewModel dvm)
        {
            dvm.Editable.Committed += (s, e) =>
            {
                var doctor = e.entity as Doctor;
                repository.SaveOrUpdate(doctor);
            };
            dvm.Editable.DirtyChanged += (s, e) =>
            {
                var doc = e.entity as Doctor;
                this.Send((int)EventID.CurrentDoctorChanged, new DoctorModelParams(doc).Params);
            };
        }
    }
}