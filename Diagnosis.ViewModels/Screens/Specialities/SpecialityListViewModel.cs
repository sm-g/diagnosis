using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class SpecialityListViewModel : ScreenBaseViewModel
    {
        private Speciality _current;
        private ObservableCollection<Speciality> _specialities;
        private SpecialityEditorViewModel _editor;
        private Saver saver;

        public SpecialityListViewModel()
        {
            Title = "Специальности";

            saver = new Saver(Session);

            var specs = Session.QueryOver<Speciality>().List();
            Specialities.SyncWith(specs);
        }

        public ObservableCollection<Speciality> Specialities
        {
            get
            {
                if (_specialities == null)
                {
                    _specialities = new ObservableCollection<Speciality>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_specialities);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _specialities;
            }
        }

        public Speciality SelectedSpeciality
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
                    OnPropertyChanged(() => SelectedSpeciality);
                }
            }
        }

        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var title = "";
                    if (SelectedSpeciality != null)
                        title = SelectedSpeciality.Title;

                    Editor = new SpecialityEditorViewModel(new Speciality(title));
                    Editor.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "DialogResult")
                        {
                            if (!Editor.Speciality.spec.IsTransient)
                                SelectLastChanged(Editor.Speciality.spec);
                            Editor.Dispose();
                            Editor = null;
                        }
                    };
                });
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Editor = new SpecialityEditorViewModel(SelectedSpeciality);
                    Editor.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "DialogResult")
                        {
                            SelectLastChanged(Editor.Speciality.spec);
                            Editor.Dispose();
                            Editor = null;
                        }
                    };
                }, () => SelectedSpeciality != null);
            }
        }

        public SpecialityEditorViewModel Editor
        {
            get
            {
                return _editor;
            }
            set
            {
                if (_editor != value)
                {
                    _editor = value;
                    OnPropertyChanged(() => Editor);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        private void SelectLastChanged(Speciality spec)
        {
            if (!Specialities.Contains(spec))
                Specialities.Add(spec);
            SelectedSpeciality = spec;
        }
    }
}