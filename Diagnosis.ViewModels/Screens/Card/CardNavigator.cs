using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CardNavigator : AbstractNavigatorViewModel<CardItemViewModel, Patient, Course, Appointment, IHrsHolder>
    {
        public CardNavigator(HierViewer<Patient, Course, Appointment, IHrsHolder> viewer)
            : base(viewer)
        {
        }

        protected override string GetCurrentPathDescription(HierViewer<Patient, Course, Appointment, IHrsHolder> viewer, CardItemViewModel current)
        {
            string delim = " \\ ";
            string result = NameFormatter.GetFullName(viewer.OpenedRoot) ?? string.Format("Пациент ({0:dd.MM.yy hh:mm})", viewer.OpenedRoot.CreatedAt);

            var h = current.Holder;
            if (h is Course)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedMiddle.Start, viewer.OpenedMiddle.End);
            }
            else if (h is Appointment)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedMiddle.Start, viewer.OpenedMiddle.End);
                result += delim + "осмотр " + DateFormatter.GetDateString(viewer.OpenedLeaf.DateAndTime);
            }
            return result;
        }

        public override void AddRootItemFor(IHrsHolder holder)
        {
            var p = holder.GetPatient();
            if (!roots.Contains(p))
            {
                // first create ItemVm
                var itemVm = new CardItemViewModel(p);
                roots.Add(p);
                TopItems.Add(itemVm);
            }
        }

        protected override void CurrentChanging()
        {
            TopItems.ForAll(x => HightlightLastOpenedFor(x));

            // close nested for saving
            var holder = Current.Holder;
            if (holder is Patient)
                viewer.OpenedMiddle = null;
            else if (holder is Course)
                viewer.OpenedLeaf = null;
        }

        protected internal override CardItemViewModel FindItemVmOf(IHrsHolder node)
        {
            return TopItems.FindHolderKeeperOf(node);
        }

        protected override void OnRootRemoved(Patient patient)
        {
            patient.CoursesChanged -= patient_CoursesChanged;
            foreach (var item in patient.Courses)
            {
                item.AppointmentsChanged -= course_AppointmentsChanged;
            }
        }

        protected override void OnRootAdded(Patient patient)
        {
            patient.CoursesChanged += patient_CoursesChanged;
            foreach (var item in patient.Courses)
            {
                item.AppointmentsChanged += course_AppointmentsChanged;
            }
        }

        // при добавлении открываем
        // при удалении открытого открываем рядом или выше, если это был последний на уровне
        private void patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Course course;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    course = (Course)e.NewItems[0];
                    course.AppointmentsChanged += course_AppointmentsChanged;

                    OnAdded(course);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    course = (Course)e.OldItems[0];
                    course.AppointmentsChanged -= course_AppointmentsChanged;

                    if (viewer.OpenedMiddle == course)
                    {
                        viewer.Close(course);
                        if (NavigateUpperOnRemoved)
                        {
                            IHrsHolder near = viewer.OpenedRoot.Courses.ElementNear(e.OldStartingIndex);
                            NavigateTo(near ?? viewer.OpenedRoot);
                        }
                    }
                    break;
            }
        }

        private void course_AppointmentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Appointment app;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    app = (Appointment)e.NewItems[0];
                    // (сохраняется  еще при закрытии дргой страницы)

                    OnAdded(app);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    app = (Appointment)e.OldItems[0];
                    if (viewer.OpenedLeaf == app)
                    {
                        viewer.Close(app);
                        if (NavigateUpperOnRemoved)
                        {
                            IHrsHolder near = viewer.OpenedMiddle.Appointments.ElementNear(e.OldStartingIndex);
                            NavigateTo(near ?? viewer.OpenedMiddle);
                        }
                    }
                    break;
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
    }
}