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

                    OnPropertyChanged(() => CurrentDoctor);
                    this.Send((int)EventID.CurrentDoctorChanged, new CurrentDoctorChangedParams(_current).Params);
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
            foreach (var dvm in doctorVMs)
            {
                dvm.Editable.Committed += d_Committed;
            }

            Doctors = new ObservableCollection<DoctorViewModel>(doctorVMs);
            if (Doctors.Count > 0)
            {
                CurrentDoctor = Doctors[0];
            }
        }

        private void d_Committed(object sender, EditableEventArgs e)
        {
            var doctorVM = e.viewModel as DoctorViewModel;
            if (doctorVM != null)
            {
                repository.SaveOrUpdate(doctorVM.doctor);
            }
        }
    }
}