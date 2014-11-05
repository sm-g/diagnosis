using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Хранит последние загруженные курс, осмотр для пациента, курса.
    ///
    /// AutoOpen:
    /// Если пациент, курс открыты повторно, открывает последний открытый курс, осмотр соответственно.
    ///
    /// При присвоении OpenedEntity сначала закрывается текущая открытая,
    /// затем меняется свойство OpenedEntity, потом открывается новая сущность.
    ///
    /// Перед открытием и после закрытия вызывается OpenedChanged.
    /// </summary>
    public class PatientViewer : NotifyPropertyChangedBase
    {
        private Patient _openedPatient;
        private Course _openedCourse;
        private Appointment _openedApp;

        private Dictionary<Patient, Course> patCourseMap;
        private Dictionary<Course, Appointment> courseAppMap;

        public PatientViewer()
        {
            patCourseMap = new Dictionary<Patient, Course>();
            courseAppMap = new Dictionary<Course, Appointment>();
        }

        public event EventHandler<OpeningEventArgs> OpenedChanged;

        public Patient OpenedPatient
        {
            get
            {
                return _openedPatient;
            }
            internal set
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

        /// <summary>
        /// Открывать последний курс/осмотр при открытии пациента\курса.
        /// </summary>
        public bool AutoOpen { get; set; }

        public Appointment GetLastOpenedFor(Course course)
        {
            Appointment app;
            if (courseAppMap.TryGetValue(course, out app))
                return app;
            return null;
        }

        public Course GetLastOpenedFor(Patient patient)
        {
            Course course;
            if (patCourseMap.TryGetValue(patient, out course))
                return course;
            return null;
        }

        public IHrsHolder GetLastOpenedFor(IHrsHolder holder)
        {
            if (holder is Patient)
                return GetLastOpenedFor(holder as Patient);
            if (holder is Course)
                return GetLastOpenedFor(holder as Course);
            return null;
        }

        public void ClosePatient()
        {
            OpenedPatient = null;
        }

        internal void OpenPatient(Patient patient)
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

        internal void Open(IHrsHolder holder)
        {
            if (holder is Patient)
            {
                OpenPatient(holder as Patient);
            }
            else if (holder is Course)
            {
                OpenCourse(holder as Course);
            }
            else if (holder is Appointment)
            {
                OpenAppointment(holder as Appointment);
            }
        }

        /// <summary>
        /// Открывает последний за час осмотр в последнем курсе.
        /// Создает новый, если такого нет, в последнем курсе.
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

            if (AutoOpen)
            {
                Course course = GetLastOpenedFor(patient);
                if (course == null)
                {
                    OpenedCourse = patient.Courses.FirstOrDefault();
                }
                else
                {
                    OpenedCourse = course;
                }
            }
        }

        private void OnPatientClosed(Patient patient)
        {
            OpenedCourse = null;

            var e = new OpeningEventArgs(patient, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnCourseOpened(Course course)
        {
            Contract.Requires(OpenedPatient == course.Patient);
            var e = new OpeningEventArgs(course, OpeningAction.Open);
            OnOpenedChanged(e);

            if (!patCourseMap.ContainsKey(OpenedPatient))
            {
                // пациент открыт первый раз
                patCourseMap.Add(OpenedPatient, course);
            }
            else
                patCourseMap[OpenedPatient] = course;

            if (AutoOpen)
            {
                Appointment app = GetLastOpenedFor(course);
                if (app == null)
                {
                    OpenedAppointment = course.Appointments.LastOrDefault();
                }
                else
                {
                    OpenedAppointment = app;
                }
            }
        }

        private void OnCourseClosed(Course course)
        {
            OpenedAppointment = null;

            var e = new OpeningEventArgs(course, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnAppointmentOpened(Appointment app)
        {
            Contract.Requires(OpenedCourse == app.Course);
            var e = new OpeningEventArgs(app, OpeningAction.Open);
            OnOpenedChanged(e);

            if (!courseAppMap.ContainsKey(OpenedCourse))
            {
                // курс открыт первый раз
                courseAppMap.Add(OpenedCourse, app);
            }
            else
            {
                courseAppMap[OpenedCourse] = app;
            }
        }

        private void OnAppointmentClosed(Appointment app)
        {
            var e = new OpeningEventArgs(app, OpeningAction.Close);
            OnOpenedChanged(e);
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