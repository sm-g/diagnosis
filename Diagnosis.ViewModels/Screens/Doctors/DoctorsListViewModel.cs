using System.Linq;
using Diagnosis.Common;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Diagnosis.Data;
using Diagnosis.ViewModels.Search;
using NHibernate;
using NHibernate.Linq;
using Diagnosis.Data.Queries;
using System.Windows.Data;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class DoctorsListViewModel : ScreenBase
    {
        private Doctor _current;
        private ObservableCollection<Doctor> _doctors;
        private Saver saver;
        private bool CanDelete;
        private EventMessageHandlersManager emhManager;

        public DoctorsListViewModel()
        {
            saver = new Saver(Session);

            Title = "Врачи";
            var docs = Session.QueryOver<Doctor>().List()
                .OrderBy(d => d.FullName).ToList();
            var specs = Session.QueryOver<Speciality>().List()
                .OrderBy(s => s.Title).ToList();

            Doctors.SyncWith(docs);

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Events.PatientSaved, (e) =>
                {
                    // выбираем нового доктора или изменившегося
                    var doc = e.GetValue<Doctor>(MessageKeys.Doctor);
                    if (!Doctors.Contains(doc))
                        Doctors.Add(doc);
                    SelectedDoctor = doc;
                })
            });
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
                    this.Send(Events.EditDoctor, newDoc.AsParams(MessageKeys.Doctor));
                });
            }
        }
        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EditDoctor, SelectedDoctor.AsParams(MessageKeys.Doctor));
                }, () => SelectedDoctor != null);
            }
        }


        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (saver.Delete(SelectedDoctor))
                        Doctors.Remove(SelectedDoctor);
                }, () => CanDelete);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                emhManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}