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
        private HierViewer<Patient, Course, Appointment, IHrsHolder> viewer;
        private CardItemViewModel _curHolder;
        private ObservableCollection<Patient> patients;
        private IHrsHolder lastOpened;

        public NavigatorViewModel(HierViewer<Patient, Course, Appointment, IHrsHolder> viewer)
        {
            this.viewer = viewer;
            TopCardItems = new ObservableCollection<CardItemViewModel>();
            patients = new ObservableCollection<Patient>();
            viewer.OpenedChanged += viewer_OpenedChanged;
            patients.CollectionChanged += navigator_patients_CollectionChanged;
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

        public string CurrentTitle
        {
            get
            {
                if (Current == null)
                    return "";

                return GetCurrentPathDescription(viewer, Current.Holder);
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

        public static string GetCurrentPathDescription(HierViewer<Patient, Course, Appointment, IHrsHolder> viewer, IHrsHolder current)
        {
            string delim = " \\ ";
            string result = NameFormatter.GetFullName(viewer.OpenedRoot) ?? string.Format("Пациент ({0:dd.MM.yy hh:mm})", viewer.OpenedRoot.CreatedAt);

            if (current is Course)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedMiddle.Start, viewer.OpenedMiddle.End);
            }
            else if (current is Appointment)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedMiddle.Start, viewer.OpenedMiddle.End);
                result += delim + "осмотр " + DateFormatter.GetDateString(viewer.OpenedLeaf.DateAndTime);
            }
            return result;
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

            AddPatientItemFor(holder);

            lastOpened = holder; // != если viewer открывает последний осмотр

            viewer.Open(holder);
            Current = FindItemVmOf(lastOpened);
        }

        private void AddPatientItemFor(IHrsHolder holder)
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
            Current.IsExpanded = true;
            Current.ExpandParents();


            TopCardItems.ForAll(x => HightlightLastOpenedFor(x));

            // close nested for saving
            var holder = Current.Holder;
            if (holder is Patient)
                viewer.OpenedMiddle = null;
            else if (holder is Course)
                viewer.OpenedLeaf = null;

            OnPropertyChanged(() => CurrentTitle);
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

        internal CardItemViewModel FindItemVmOf(IHrsHolder holder)
        {
            return TopCardItems.FindHolderKeeperOf(holder);
        }

        /// <summary>
        /// при открытии пациента подписываемся на измение коллекций курсов и осмотров в курсах
        /// раскрываем открытый элемент дерева, сохраняем последний открытый
        ///
        /// при закрытии сворачиваем элемент дерева
        /// </summary>
        private void viewer_OpenedChanged(object sender, OpeningEventArgs<IHrsHolder> e)
        {
            logger.DebugFormat("{0} {1} {2}", e.action, e.entity.GetType().Name, e.entity);

            var holder = e.entity;
            var patient = holder as Patient;

            if (e.action == OpeningAction.Open)
            {
                if (holder is Patient)
                {
                    patient.CoursesChanged += patient_CoursesChanged;
                    foreach (var item in patient.Courses)
                    {
                        item.AppointmentsChanged += course_AppointmentsChanged;
                    }
                }

                lastOpened = holder;
                holder.PropertyChanged += holder_PropertyChanged;
            }
            else
            {
                if (holder is Patient)
                {
                    patient.CoursesChanged -= patient_CoursesChanged;
                    foreach (var item in patient.Courses)
                    {
                        item.AppointmentsChanged -= course_AppointmentsChanged;
                    }
                }
                holder.PropertyChanged -= holder_PropertyChanged;
            }
        }

        private void holder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => CurrentTitle);
        }

        private void navigator_patients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var p = (Patient)e.OldItems[0];

                if (viewer.OpenedRoot == p)
                {
                    var near = patients.ElementNear(e.OldStartingIndex);
                    NavigateTo(near); //  рядом или null
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
                if (viewer.OpenedMiddle == course)
                {
                    viewer.Close(course);
                    IHrsHolder near = viewer.OpenedRoot.Courses.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedRoot);
                }
            }
        }

        private void course_AppointmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении осмотра открываем его (сохраняется  еще при закрытии дргой страницы)
                NavigateTo((Appointment)e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var app = (Appointment)e.OldItems[0];
                // при удалении открытого осмотра открываем осмотр рядом или курс, если это был последний осмотр
                if (viewer.OpenedLeaf == app)
                {
                    viewer.Close(app);
                    IHrsHolder near = viewer.OpenedMiddle.Appointments.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedMiddle);
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