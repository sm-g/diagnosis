using Diagnosis.App.Messaging;
using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class DoctorsProducer : ViewModelBase
    {
        private DoctorViewModel _current;
        private IDoctorRepository repository;

        public ReadOnlyObservableCollection<DoctorViewModel> Doctors { get; private set; }

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
            var result = Doctors.FirstOrDefault(a => a.doctor.Id == doctor.Id); // doctors are different onjects here
            return result;
        }

        public DoctorsProducer(IDoctorRepository repo)
        {
            Contract.Requires(repo != null);
            this.repository = repo;

            var doctorVMs = repository.GetAll().Select(d => new DoctorViewModel(d)).ToList();
            doctorVMs.ForEach(dvm => SubscribeDoctor(dvm));
            Doctors = new ReadOnlyObservableCollection<DoctorViewModel>(new ObservableCollection<DoctorViewModel>(doctorVMs));

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
        }
    }
}