using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularyListViewModel : ScreenBaseViewModel
    {
        private VocabularyViewModel _current;
        private ObservableCollection<VocabularyViewModel> _vocs;
        private VocabularyEditorViewModel _editor;
        private Saver saver;

        public VocabularyListViewModel()
        {
            Contract.Requires(!Constants.IsClient);

            Title = "Специальности";

            saver = new Saver(Session);

            MakeVms();
        }

        public ObservableCollection<VocabularyViewModel> Vocs
        {
            get
            {
                if (_vocs == null)
                {
                    _vocs = new ObservableCollection<VocabularyViewModel>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_vocs);
                    SortDescription sort1 = new SortDescription("Title", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _vocs;
            }
        }

        public VocabularyViewModel SelectedVoc
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
                    OnPropertyChanged(() => SelectedVoc);
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
                    if (SelectedVoc != null)
                        title = SelectedVoc.Title;

                    Editor = new VocabularyEditorViewModel(new Vocabulary(title));
                    Editor.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "DialogResult")
                        {
                            MakeVms();

                            if (!Editor.Vocabulary.voc.IsTransient)
                                SelectLastChanged(Editor.Vocabulary.voc);
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
                    Editor = new VocabularyEditorViewModel(SelectedVoc.voc);
                    Editor.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "DialogResult")
                        {
                            SelectLastChanged(Editor.Vocabulary.voc);
                            Editor.Dispose();
                            Editor = null;
                        }
                    };
                }, () => SelectedVoc != null);
            }
        }

        public VocabularyEditorViewModel Editor
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
        private void MakeVms()
        {
            var vocs = Session.QueryOver<Vocabulary>().List();

            var vms = vocs.Select(v => Vocs
                .Where(vm => vm.voc == v)
                .FirstOrDefault() ?? new VocabularyViewModel(v));

            Vocs.SyncWith(vms);
        }
        private void SelectLastChanged(Vocabulary voc)
        {
            SelectedVoc = Vocs.FirstOrDefault(x => x.voc == voc);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}