using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Diagnosis.ViewModels
{
    public class AppointmentsManager
    {
        private readonly Course course;
        private ObservableCollection<SpecialCaseItem> _appointments;

        public event EventHandler AppointmentsLoaded;

        public ObservableCollection<SpecialCaseItem> Appointments
        {
            get
            {
                if (_appointments == null)
                {
                    IList<SpecialCaseItem> wrappers;
                    using (var tester = new PerformanceTester((ts) => Debug.Print("making apps for {0}: {1}", course, ts)))
                    {
                        wrappers = course.Appointments
                            .Select(app => new ShortAppointmentViewModel(app, app.Doctor == course.LeadDoctor))
                            .Select(vm => new SpecialCaseItem(vm))
                            .ToList();
                    }

                    _appointments = new ObservableCollection<SpecialCaseItem>(wrappers);
                    if (!course.End.HasValue)
                    {
                        _appointments.Add(new SpecialCaseItem(SpecialCaseItem.Cases.AddNew));
                    }

                    SetAppointmentsDeletable();
                    OnAppointmentsLoaded();
                }
                return _appointments;
            }
        }

        public AppointmentsManager(Course course)
        {
            this.course = course;
            course.Appointments.CollectionChanged2 += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (Appointment item in e.NewItems)
                    {
                        var appVM = new ShortAppointmentViewModel(item, item.Doctor == course.LeadDoctor);
                        var wrapper = new SpecialCaseItem(appVM);
                        Appointments.Insert(Appointments.Count - 1, wrapper);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (Appointment item in e.OldItems)
                    {
                        var appVM = Appointments.Where(w => w.To<ShortAppointmentViewModel>().appointment == item).FirstOrDefault();
                        Appointments.Remove(appVM);
                    }
                }
            };
        }

        protected virtual void OnAppointmentsLoaded()
        {
            var h = AppointmentsLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        private void SetAppointmentsDeletable()
        {
            //// проверяем, остался ли курс пустым
            //course.Editable.CanBeDeleted = course.IsEmpty;

            //// нельзя удалять единственный непустой осмотр
            //if (Appointments.Count > 1)
            //{
            //    Appointments.ForAll(app => app.Editable.CanBeDeleted = true);
            //}
            //else if (Appointments.Count == 1)
            //{
            //    Appointments[0].Editable.CanBeDeleted = Appointments[0].IsEmpty;
            //}
        }


    }
}