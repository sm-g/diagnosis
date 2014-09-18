using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Обрабатывает действия при открытии и закрытии пациента, курса, осмотра, записи.
    ///
    /// Если пациент, курс, осмотр открыты повторно, открывает последний открытый курс, осмотр, запись соответственно.
    ///
    /// Если в открываемых первый раз курсе нет осмотров или в осмотре нет записей, добавляет их.
    ///
    /// При присвоении OpenedEntity сначала закрывается текущая открытая,
    /// затем меняется свойство OpenedEntity, потом открывается новая сущность.
    ///
    /// Перед открытием и после закрытия вызывается OpenedChanged.
    /// </summary>
    public class PatientViewer : NotifyPropertyChangedBase
    {
        private Patient _openedPatient;
        private Appointment _openedApp;
        private Course _openedCourse;
        private HealthRecord _openedHr;

        // последние открытые
        private Dictionary<Patient, Course> patCourseMap;
        private Dictionary<Course, Appointment> courseAppMap;
        private Dictionary<Appointment, HealthRecord> appHrMap;

        public PatientViewer()
        {
            patCourseMap = new Dictionary<Patient, Course>();
            courseAppMap = new Dictionary<Course, Appointment>();
            appHrMap = new Dictionary<Appointment, HealthRecord>();
        }

        public event EventHandler<OpeningEventArgs> OpenedChanged;

        public Patient OpenedPatient
        {
            get
            {
                return _openedPatient;
            }
            private set
            {
                if (_openedPatient != value)
                {
                    if (_openedPatient != null)
                    {
                        OnPatientClosed(_openedPatient);
                    }
                    _openedPatient = value;

                    OnPropertyChanged("OpenedPatient");
                    if (value != null)
                    {
                        OnPatientOpened(value);
                    }
                }
            }
        }

        public Course OpenedCourse
        {
            get
            {
                return _openedCourse;
            }
            internal set
            {
                if (_openedCourse != value)
                {
                    if (_openedCourse != null)
                    {
                        OnCourseClosed(_openedCourse);
                    }

                    _openedCourse = value;

                    OnPropertyChanged("OpenedCourse");
                    if (value != null)
                    {
                        OnCourseOpened(value);
                    }
                }
            }
        }

        public Appointment OpenedAppointment
        {
            get
            {
                return _openedApp;
            }
            internal set
            {
                if (_openedApp != value)
                {
                    if (_openedApp != null)
                    {
                        OnAppointmentClosed(_openedApp);
                    }

                    _openedApp = value;

                    OnPropertyChanged("OpenedAppointment");
                    if (value != null)
                    {
                        OnAppointmentOpened(value);
                    }
                }
            }
        }

        public HealthRecord OpenedHealthRecord
        {
            get
            {
                return _openedHr;
            }
            internal set
            {
                if (_openedHr != value)
                {
                    if (_openedHr != null)
                    {
                        OnHrClosed(_openedHr, value);
                    }
                    _openedHr = value;

                    if (value != null)
                    {
                        OnHrOpened(value);
                    }

                    OnPropertyChanged("OpenedHealthRecord");
                }
            }
        }

        public void ClosePatient()
        {
            OpenedHealthRecord = null;
            OpenedAppointment = null;
            OpenedCourse = null;
            OpenedPatient = null;
        }

        public void OpenPatient(Patient patient)
        {
            OpenedPatient = patient;
        }

        internal void OpenCourse(Course course)
        {
            OpenedPatient = course.Patient;
            OpenedCourse = course;
        }

        internal void OpenAppointment(Appointment app)
        {
            OpenedPatient = app.Course.Patient;
            OpenedCourse = app.Course;
            OpenedAppointment = app;
        }

        internal void OpenHr(HealthRecord hr)
        {
            OpenedPatient = hr.Appointment.Course.Patient;
            OpenedCourse = hr.Appointment.Course;
            OpenedAppointment = hr.Appointment;
            OpenedHealthRecord = hr;
        }

        /// <summary>
        /// Открывает последний за час осмотр в последнем курсе.
        /// Создает новую, если такой нет, в последнем курсе.
        /// Создает курс, если нет ни одного курса.
        /// </summary>
        /// <param name="patient"></param>
        public void OpenLastAppointment(Patient patient)
        {
            OpenPatient(patient);

            // последний курс или новый, если курсов нет
            var lastCourse = patient.Courses.FirstOrDefault();
            if (lastCourse == null)
            {
                AuthorityController.CurrentDoctor.StartCourse(patient);
            }
            else
            {
                OpenedCourse = lastCourse;
            }

            // последний осмотр в течение часа или новый
            var lastApp = OpenedCourse.Appointments.LastOrDefault();
            if (lastApp != null && DateTime.UtcNow - lastApp.DateAndTime > TimeSpan.FromHours(1))
            {
                OpenedCourse.AddAppointment(null);
            }
            else
            {
                OpenedAppointment = lastApp;
            }
        }

        private void OnPatientOpened(Patient patient)
        {
            var e = new OpeningEventArgs(patient, OpeningAction.Open);
            OnOpenedChanged(e);

            patient.CoursesChanged += Courses_CollectionChanged;

            Course course;
            if (!patCourseMap.TryGetValue(patient, out course))
            {
                // пациент открыт первый раз
                OpenedCourse = patient.Courses.FirstOrDefault();
            }
            else
            {
                // пацинет открыт повторно
                OpenedCourse = course;
            }
        }

        private void OnPatientClosed(Patient patient)
        {
            patient.CoursesChanged -= Courses_CollectionChanged;
            OpenedCourse = null;

            var e = new OpeningEventArgs(patient, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnCourseOpened(Course course)
        {
            Contract.Requires(OpenedPatient == course.Patient);
            var e = new OpeningEventArgs(course, OpeningAction.Open);
            OnOpenedChanged(e);

            course.AppointmentsChanged += Appointments_CollectionChanged;

            if (!patCourseMap.ContainsKey(OpenedPatient))
            {
                // пациент открыт первый раз
                patCourseMap.Add(OpenedPatient, course);
            }
            else
                patCourseMap[OpenedPatient] = course;

            Appointment app;
            if (!courseAppMap.TryGetValue(course, out app))
            {
                // курс открыт первый раз
                OpenedAppointment = course.Appointments.LastOrDefault();
            }
            else
            {
                OpenedAppointment = app;
            }
        }

        private void OnCourseClosed(Course course)
        {
            course.AppointmentsChanged -= Appointments_CollectionChanged;
            OpenedAppointment = null;

            var e = new OpeningEventArgs(course, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnAppointmentOpened(Appointment app)
        {
            Contract.Requires(OpenedCourse == app.Course);
            var e = new OpeningEventArgs(app, OpeningAction.Open);
            OnOpenedChanged(e);

            app.HealthRecordsChanged += HealthRecords_CollectionChanged;

            if (!courseAppMap.ContainsKey(OpenedCourse))
            {
                // курс открыт первый раз
                courseAppMap.Add(OpenedCourse, app);
            }
            else
            {
                courseAppMap[OpenedCourse] = app;
            }

            HealthRecord hr;
            if (!appHrMap.TryGetValue(app, out hr))
            {
                // осмотр открыт первый раз
                // никакие записи не выбраны
                // если записей нет, добавляется новая и открывается на редактирование в CardVM
            }
            else
            {
                // повторно — выбрана последняя открытая запись
                OpenedHealthRecord = hr;
            }
        }

        private void OnAppointmentClosed(Appointment app)
        {
            app.HealthRecordsChanged -= HealthRecords_CollectionChanged;
            OpenedHealthRecord = null;

            var e = new OpeningEventArgs(app, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnHrOpened(HealthRecord hr)
        {
            Contract.Requires(OpenedAppointment == hr.Appointment);
            var e = new OpeningEventArgs(hr, OpeningAction.Open);
            OnOpenedChanged(e);

            if (!appHrMap.ContainsKey(OpenedAppointment))
            {
                // осмотр открыт первый раз
                appHrMap.Add(OpenedAppointment, hr);
            }
            else
            {
                appHrMap[OpenedAppointment] = hr;
            }
        }

        private void OnHrClosed(HealthRecord closing, HealthRecord opening)
        {
            var e = new OpeningEventArgs(closing, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void Courses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении курса открываем его
                OpenedCourse = (Course)e.NewItems[e.NewItems.Count - 1];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении курса открываем курс рядом с удаленным
                var i = e.OldStartingIndex;
                if (OpenedPatient.Courses.Count() <= i)
                    i--;
                if (OpenedPatient.Courses.Count() > 0)
                {
                    OpenedCourse = OpenedPatient.Courses.ElementAt(i);
                }
            }
        }

        private void Appointments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении осмотра открываем его
                OpenedAppointment = (Appointment)e.NewItems[e.NewItems.Count - 1];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении осмотра открываем последний осмотр
                if (OpenedAppointment == null && OpenedCourse.Appointments.Count() > 0)
                {
                    OpenedAppointment = OpenedCourse.Appointments.LastOrDefault();
                }
            }
        }

        private void HealthRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
            }
        }

        protected virtual void OnOpenedChanged(OpeningEventArgs e)
        {
            var h = OpenedChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        [Serializable]
        public class OpeningEventArgs : EventArgs
        {
            public readonly OpeningAction action;
            public readonly object entity;

            [DebuggerStepThrough]
            public OpeningEventArgs(object entity, OpeningAction action)
            {
                this.action = action;
                this.entity = entity;
            }
        }

        public enum OpeningAction
        {
            Open, Close
        }
    }
}