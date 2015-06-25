using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorsListViewModel : ScreenBaseViewModel
    {
        private Doctor _current;
        private ObservableCollection<Doctor> _doctors;
        private bool CanDelete;

        public DoctorsListViewModel()
        {

            Title = "Врачи";
            var docs = EntityQuery<Doctor>.All(Session)()
                .OrderBy(d => d.FullName).ToList();
            var specs = EntityQuery<Speciality>.All(Session)()
                .OrderBy(s => s.Title).ToList();

            Doctors.SyncWith(docs);

            emh.Add(this.Subscribe(Event.EntitySaved, (e) =>
                {
                    // выбираем нового доктора или изменившегося
                    var doc = e.GetValue<IEntity>(MessageKeys.Entity) as Doctor;
                    if (doc != null)
                        SelectDoctor(doc);
                }
            ));
        }

        public ObservableCollection<Doctor> Doctors
        {
            get
            {
                if (_doctors == null)
                {
                    _doctors = new ObservableCollection<Doctor>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_doctors);
                    //SortDescription sort1 = new SortDescription("FullName", ListSortDirection.Ascending);
                    //view.SortDescriptions.Add(sort1);
                }
                return _doctors;
            }
        }

        public Doctor SelectedDoctor
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

                    CanDelete = _current != null && _current.IsEmpty();
                    OnPropertyChanged(() => SelectedDoctor);
                }
            }
        }

        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    object newDoc = null;
                    this.Send(Event.EditDoctor, newDoc.AsParams(MessageKeys.Doctor));
                });
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditDoctor, SelectedDoctor.AsParams(MessageKeys.Doctor));
                }, () => SelectedDoctor != null);
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Session.DoDelete(SelectedDoctor))
                        Doctors.Remove(SelectedDoctor);
                }, () => CanDelete);
            }
        }

        private void SelectDoctor(Doctor doc)
        {
            Contract.Requires(doc != null);

            if (!Doctors.Contains(doc))
                Doctors.Add(doc);
            SelectedDoctor = doc;
        }
    }
}