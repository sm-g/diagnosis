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
                roots.Add(p);
                var itemVm = new CardItemViewModel(p);
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

        protected override void OnRootClosed(Patient patient)
        {
            patient.CoursesChanged -= patient_CoursesChanged;
            foreach (var item in patient.Courses)
            {
                item.AppointmentsChanged -= course_AppointmentsChanged;
            }
        }

        protected override void OnRootOpened(Patient patient)
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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var course = (Course)e.NewItems[0];
                course.AppointmentsChanged += course_AppointmentsChanged;

                NavigateTo(course);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var course = (Course)e.OldItems[0];
                course.AppointmentsChanged -= course_AppointmentsChanged;

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
                // (сохраняется  еще при закрытии дргой страницы)
                NavigateTo((Appointment)e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var app = (Appointment)e.OldItems[0];
                if (viewer.OpenedLeaf == app)
                {
                    viewer.Close(app);
                    IHrsHolder near = viewer.OpenedMiddle.Appointments.ElementNear(e.OldStartingIndex);
                    NavigateTo(near ?? viewer.OpenedMiddle);
                }
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
