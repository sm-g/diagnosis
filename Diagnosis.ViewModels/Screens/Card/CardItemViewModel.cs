using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Data;
using NHibernate;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Diagnosis.ViewModels.Screens
{
    public class CardItemViewModel : HierarchicalBase<CardItemViewModel>
    {
        private bool _isHighlighted;

        public CardItemViewModel(IHrsHolder holder)
        {
            Holder = holder;
            if (holder is Patient)
            {
                var patient = holder as Patient;
                var courseVMs = patient.Courses
                       .OrderByDescending(c => c, new CompareCourseByDate())
                       .Select(i => new CardItemViewModel(i.Actual as Course))
                       .ToList();
                foreach (var item in courseVMs)
                {
                    Children.Add(item);
                }
                patient.CoursesChanged += nested_IHrsHolders_Changed;
            }
            if (holder is Course)
            {
                var course = holder as Course;

                var appVMs = course.Appointments.Select(app => new CardItemViewModel(app.Actual as Appointment));

                foreach (var item in appVMs)
                {
                    Children.Add(item);
                }

                course.AppointmentsChanged += nested_IHrsHolders_Changed;
            }
        }

        public IHrsHolder Holder { get; private set; }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.OpenHolder, Holder.AsParams(MessageKeys.Holder));
                });
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    IDialog vm = null;
                    if (Holder is Course)
                    {
                        vm = new CourseEditorViewModel(Holder as Course);
                        this.Send(Events.OpenHolderEditor, vm.AsParams(MessageKeys.Dialog));
                    }
                    else if (Holder is Patient)
                    {
                        this.Send(Events.EditPatient, Holder.AsParams(MessageKeys.Patient));
                    }
                });
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EntityDeleted, Holder.AsParams(MessageKeys.Holder));
                }, () => Holder.IsEmpty());
            }
        }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AuthorityController.CurrentDoctor.StartCourse(Holder as Patient);
                },
                () => Holder is Patient);
            }
        }

        public RelayCommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    (Holder as Course).AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => Holder is Course && (Holder as Course).End == null);
            }
        }

        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged(() => IsHighlighted);
                }
            }
        }
        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();
            if (IsSelected)
            {
                ExpandParents();
            }
        }

        private void ExpandParents()
        {
            if (Parent != null)
            {
                Parent.IsExpanded = true;
                Parent.ExpandParents();
            }
        }

        private void nested_IHrsHolders_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IHrsHolder item in e.NewItems)
                {
                    var vm = new CardItemViewModel(item);
                    Children.Add(vm);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (IHrsHolder item in e.OldItems)
                {
                    var vm = Children.Where(w => w.Holder == item).FirstOrDefault();
                    Children.Remove(vm);
                    vm.Dispose();
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Holder is Patient)
                    {
                        (Holder as Patient).CoursesChanged -= nested_IHrsHolders_Changed;
                    }
                    if (Holder is Course)
                    {
                        (Holder as Course).AppointmentsChanged -= nested_IHrsHolders_Changed;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.GetType(), Holder);
        }
    }
}