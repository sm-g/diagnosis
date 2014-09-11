using Diagnosis.Core;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using NHibernate;

namespace Diagnosis.ViewModels
{
    /// <summary>
    /// Обрабатывает действия при открытии и закрытии пациента, курса, осмотра, записи.
    /// </summary>
    public class PatientViewer : NotifyPropertyChangedBase
    {
        private Patient _openedPatient;
        private Appointment _openedApp;
        private Course _openedCourse;
        private HealthRecord _openedHr;
        private bool _fastAddingMode;

        // последние открытые
        private Dictionary<Patient, Course> patCourseMap;
        private Dictionary<Course, Appointment> courseAppMap;
        private Dictionary<Appointment, HealthRecord> appHrMap;
        private bool supressCourseClosing;

        public PatientViewer()
        {
            patCourseMap = new Dictionary<Patient, Course>();
            courseAppMap = new Dictionary<Course, Appointment>();
            appHrMap = new Dictionary<Appointment, HealthRecord>();
        }

        public ISession Session { get; set; }

        public bool FastAddingMode
        {
            get
            {
                return _fastAddingMode;
            }
            set
            {
                if (_fastAddingMode != value)
                {
                    _fastAddingMode = value;
                    OnPropertyChanged(() => FastAddingMode);
                }
            }
        }

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
                        OnPatientClosed(_openedPatient, value);
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
            set
            {
                if (_openedCourse != value && !supressCourseClosing)
                {
                    if (_openedCourse != null)
                    {
                        OnCourseClosed(_openedCourse);
                    }

                    _openedCourse = value;

                    OnPropertyChanged(() => OpenedCourse);
                    if (value != null)
                    {
                        OnCourseOpened(value);
                    }
                }
                if (supressCourseClosing)
                {
                    supressCourseClosing = false;
                    OnPropertyChanged(() => OpenedCourse);
                }
            }
        }

        public Appointment OpenedAppointment
        {
            get
            {
                return _openedApp;
            }
            set
            {
                if (_openedApp != value)
                {
                    if (_openedApp != null)
                    {
                        OnAppointmentClosed(_openedApp);
                    }

                    _openedApp = value;

                    OnPropertyChanged(() => OpenedAppointment);
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
            set
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
                    else
                    {
                        Debug.Print("нет открытых записей");
                    }

                    OnPropertyChanged(() => OpenedHealthRecord);
                }
            }
        }

        public void ClosePatient()
        {
            OpenedPatient = null;
        }

        public void OpenPatient(Patient patient, bool addFirstHr = false)
        {
            OpenedPatient = patient;
            if (addFirstHr)
            {
                OpenLastAppointment(patient);
            }
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

        internal void OpenHr(HealthRecord hr)
        {
            OpenedPatient = hr.Appointment.Course.Patient;
            OpenedCourse = hr.Appointment.Course;
            OpenedAppointment = hr.Appointment;
            OpenedHealthRecord = hr;
        }

        private void OnPatientOpened(Patient patient)
        {
            Debug.Print("opening patient {0}", patient);

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

            if (FastAddingMode && patient.Id != 0)
            {
                OpenLastAppointment(patient);
            }
            Debug.Print("opened patient {0}", patient);
        }

        private void OnPatientClosed(Patient closing, Patient opening)
        {
            Debug.Print("closing patient {0}", _openedPatient);
            closing.CoursesChanged -= Courses_CollectionChanged;

           // Session.SaveOrUpdate(closing);


            OpenedCourse = null;

            if (opening != null)
            {
                if (opening.Id != 0)
                {
                    // сохраняем состояние редактора при смене пациента
                    // opening.Editable.IsEditorActive = closing.Editable.IsEditorActive;
                }
            }

            Debug.Print("closed patient {0}", _openedPatient);
        }

        private void OnCourseOpened(Course course)
        {
            Debug.Print("opening course {0}", course);

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

                if (OpenedCourse.Appointments.Count() == 0)
                {
                    OpenedCourse.AddAppointment(AuthorityController.CurrentDoctor); // новый курс — добавляем осмотр
                }

                OpenedAppointment = OpenedCourse.Appointments.LastOrDefault();
            }
            else
            {
                OpenedAppointment = app;
            }
            Debug.Print("opened course {0}", course);

        }

        private void OnCourseClosed(Course course)
        {
            Debug.Print("closing course {0}", course);

            //Session.SaveOrUpdate(course);

            course.AppointmentsChanged -= Appointments_CollectionChanged;
            OpenedAppointment = null;

            Debug.Print("closed course {0}", course);
        }

        private void OnAppointmentOpened(Appointment app)
        {
            Contract.Requires(OpenedCourse == app.Course);
            Debug.Print("opening app  {0}", app);

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

            HealthRecord hrVm;
            if (!appHrMap.TryGetValue(app, out hrVm))
            {
                // осмотр открыт первый раз

                if (OpenedAppointment.HealthRecords.Count() == 0)
                {
                    // новый осмотр - добавляем запись
                    OpenedAppointment.AddHealthRecord();
                }
                else
                {
                    // никакие записи не выбраны
                    OpenedHealthRecord = null;
                }
            }
            else
            {
                // повторно — выбрана последняя запись, редактор закрыт
                OpenedHealthRecord = hrVm;
                //OpenedHealthRecord.Editable.IsEditorActive = false;
            }
            Debug.Print("opened app  {0}", app);
        }

        private void OnAppointmentClosed(Appointment app)
        {
            Debug.Print("closing app {0}", app);

            app.HealthRecordsChanged -= HealthRecords_CollectionChanged;

            OpenedHealthRecord = null;
            Debug.Print("closed app {0}", app);
        }

        private void OnHrOpened(HealthRecord hr)
        {
            Debug.Print("opening hr {0}", hr);

            if (!appHrMap.ContainsKey(OpenedAppointment))
            {
                // осмотр открыт первый раз
                appHrMap.Add(OpenedAppointment, hr);
            }
            else
            {
                appHrMap[OpenedAppointment] = hr;
            }

            Debug.Print("opened hr {0}", hr);
        }

        private void OnHrClosed(HealthRecord closing, HealthRecord opening)
        {
            Debug.Print("closing hr {0}", closing);

            //// новая выбранная запись в том же приеме
            //if (opening != null && closing.Appointment == opening.Appointment)
            //{
            //    // оставляем редактор открытым при смене выбранной записи
            //    if (closing.Editable.IsEditorActive)
            //    {
            //        opening.Editable.IsEditorActive = closing.Editable.IsEditorActive;
            //    }
            //}
            //closing.Editable.IsEditorActive = false;
           // Session.SaveOrUpdate(closing);

            Debug.Print("closed hr {0}", closing);
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
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                supressCourseClosing = true;
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
                // открываем добавленную запись на редактирование
                OpenedHealthRecord = (HealthRecord)e.NewItems[e.NewItems.Count - 1];
                //OpenedHealthRecord.Editable.IsEditorActive = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
            }
        }
    }
}