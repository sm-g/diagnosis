using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class NavigatorViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(NavigatorViewModel));
        private PatientViewer viewer;
        private CardItemViewModel _curHolder;
        private bool _closeNestedOnLevelUp;
        private ObservableCollection<Patient> patients;
        private IHrsHolder lastOpened;

        public NavigatorViewModel(PatientViewer viewer)
        {
            this.viewer = viewer;
            TopCardItems = new ObservableCollection<CardItemViewModel>();
            patients = new ObservableCollection<Patient>();
            viewer.OpenedChanged += viewer_OpenedChanged;
            patients.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var p = (Patient)e.OldItems[0];

                    if (viewer.OpenedPatient == p)
                    {
                        var near = patients.ElementNear(e.OldStartingIndex);
                        NavigateTo(near);
                    }
                }
            };
        }

        public event EventHandler<HrsHolderEventArgs> CurrentChanged;

        public event EventHandler<HrsHolderEventArgs> Navigating;

        public CardItemViewModel Current
        {
            get
            {
                return _curHolder;
            }
            private set
            {
                if (_curHolder != value)
                {
                    _curHolder = value;

                    OnCurrentHolderChanged();

                    OnPropertyChanged(() => Current);
                    OnCurrentChanged(new HrsHolderEventArgs(value != null ? value.Holder : null));
                }
            }
        }

        /// <summary>
        /// Пациенты в корне дерева.
        /// </summary>
        public ObservableCollection<CardItemViewModel> TopCardItems
        {
            get;
            private set;
        }

        public void NavigateTo(IHrsHolder holder)
        {
            OnNavigating(new HrsHolderEventArgs(holder));
            if (holder == null)
            {
                viewer.CloseAll();
                Current = null;
                return;
            }

            Add(holder);

            lastOpened = holder; // != если viewer открывает последний осмотр

            viewer.Open(holder);
            Current = FindItemVmOf(lastOpened);
        }

        public void Add(IHrsHolder holder)
        {
            var p = holder.GetPatient();
            if (!patients.Contains(p))
            {
                patients.Add(p);
                var itemVm = new CardItemViewModel(p);
                TopCardItems.Add(itemVm);
            }
        }

        public void Remove(Patient p)
        {
            if (patients.Remove(p))
            {
                var itemVm = FindItemVmOf(p);
                TopCardItems.Remove(itemVm);
            }
        }

        private void OnCurrentHolderChanged()
        {
            if (Current == null)
                return;

            Current.IsSelected = true;

            var holder = Current.Holder;

            TopCardItems.ForAll(x => HightlightLastOpenedFor(x));

            // close nested for saving
            if (holder is Patient)
            {
                viewer.OpenedCourse = null;
            }
            else if (holder is Course)
            {
                viewer.OpenedAppointment = null;
            }
        }

        private void HightlightLastOpenedFor(CardItemViewModel vm)
        {
            vm.Children.ForAll(x => x.IsHighlighted = false);
            var holder = viewer.GetLastOpenedFor(vm.Holder);
            var item = vm.Children.Where(x => x.Holder == holder).FirstOrDefault();
            if (item != null && item != Current)
            {
                item.IsHighlighted = true;
            }
            vm.Children.ForAll(x => HightlightLastOpenedFor(x));
        }

        private CardItemViewModel FindItemVmOf(IHrsHolder holder)
        {
            holder = holder.Actual as IHrsHolder;
            CardItemViewModel vm;
            foreach (var item in TopCardItems)
            {
                if (item.Holder == holder)
                    return item;
                vm = item.AllChildren.Where(x => x.Holder == holder).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }

        /// <summary>
        /// при открытии пациента подписываемся на измение коллекций курсов и осмотров в курсах
        /// раскрываем открытый элемент дерева, сохраняем последний открытый
        ///
        /// при закрытии сворачиваем элемент дерева
        /// </summary>
        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            Contract.Requires(e.entity is IHrsHolder);
            logger.DebugFormat("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            CardItemViewModel itemVm;
            var holder = e.entity as IHrsHolder;
            var patient = holder as Patient;

            if (e.action == PatientViewer.OpeningAction.Open)
            {
                if (holder is Patient)
                {
                    patient.CoursesChanged += patient_CoursesChanged;
                    foreach (var item in patient.Courses)
                    {
                        item.AppointmentsChanged += course_AppointmentsChanged;
                    }
                }
                itemVm = FindItemVmOf(holder);
                itemVm.IsExpanded = true;

                lastOpened = holder;
            }
            else
            {
                itemVm = FindItemVmOf(holder);

                if (holder is Patient)
                {
                    patient.CoursesChanged -= patient_CoursesChanged;
                    foreach (var item in patient.Courses)
                    {
                        item.AppointmentsChanged -= course_AppointmentsChanged;
                    }
                }

                if (itemVm != null)
                {
                    itemVm.IsExpanded = false;
                }
            }
        }

        private void patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var course = (Course)e.NewItems[0];
                course.AppointmentsChanged += course_AppointmentsChanged;
                // при добавлении курса открываем его
                NavigateTo(course);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var course = (Course)e.OldItems[0];
                course.AppointmentsChanged -= course_AppointmentsChanged;

                // при удалении открытого курса открываем курс рядом с удаленным или пациента, если это был последний курс
                if (viewer.OpenedCourse == course)
                {
                    var near = viewer.OpenedPatient.Courses.ElementNear(e.OldStartingIndex);
                    if (near == null)
                    {
                        viewer.OpenedCourse = null;
                        NavigateTo(viewer.OpenedPatient);
                    }
                    else
                        NavigateTo(near);
                }
            }
        }

        private void course_AppointmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении осмотра открываем его
                NavigateTo((Appointment)e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var app = (Appointment)e.OldItems[0];
                // при удалении открытого осмотра открываем осмотр рядом или курс, если это был последний осмотр
                if (viewer.OpenedAppointment == app)
                {
                    var near = viewer.OpenedCourse.Appointments.ElementNear(e.OldStartingIndex);
                    if (near == null)
                    {
                        viewer.OpenedAppointment = null;
                        NavigateTo(viewer.OpenedCourse);
                    }
                    else
                        NavigateTo(near);
                }
            }
        }

        protected virtual void OnCurrentChanged(HrsHolderEventArgs e)
        {
            logger.DebugFormat("Current is {0}", e.holder);

            var h = CurrentChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected virtual void OnNavigating(HrsHolderEventArgs e)
        {
            logger.DebugFormat("Navigating to {0}", e.holder);

            var h = Navigating;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                viewer.OpenedChanged -= viewer_OpenedChanged;
            }
            base.Dispose(disposing);
        }
    }
}