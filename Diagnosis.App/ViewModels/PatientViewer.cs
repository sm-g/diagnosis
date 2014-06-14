using Diagnosis.App.Messaging;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class PatientViewer : ViewModelBase
    {
        private readonly DoctorViewModel doctor;
        private PatientViewModel _openedPatient;
        private AppointmentViewModel _openedApp;
        private CourseViewModel _openedCourse;
        private HealthRecordViewModel _openedHr;
        private bool _fastAddingMode;
        private Dictionary<PatientViewModel, CourseViewModel> patCourseMap;
        private Dictionary<CourseViewModel, AppointmentViewModel> courseAppMap;
        private bool supressCourseClosing;

        public PatientViewer(DoctorViewModel doctor)
        {
            this.doctor = doctor;
            patCourseMap = new Dictionary<PatientViewModel, CourseViewModel>();
            courseAppMap = new Dictionary<CourseViewModel, AppointmentViewModel>();
        }

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

        public PatientViewModel OpenedPatient
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
                        Console.WriteLine("пациент {0} закрыт", _openedPatient);

                        if (value != null)
                        {
                            if (!(value is UnsavedPatientViewModel))
                            {
                                // сохраняем состояние редактора при смене пациента
                                value.Editable.IsEditorActive = _openedPatient.Editable.IsEditorActive;
                            }
                        }
                        else
                        {
                            UnsubscribeSelectedHr();
                            Console.WriteLine("current patient removed");
                        }
                    }
                    _openedPatient = value;

                    if (value != null)
                    {
                        OnPatientOpened(value);
                        Console.WriteLine("пациент {0} открыт", value);
                    }
                    OnPropertyChanged("OpenedPatient");
                    this.Send((int)EventID.OpenedPatientChanged, new PatientParams(OpenedPatient).Params);
                }
            }
        }

        public CourseViewModel OpenedCourse
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
                        Console.WriteLine("курс {0} закрыт", _openedCourse);
                    }

                    _openedCourse = value;

                    if (value != null)
                    {
                        OnCourseOpened(value);
                        Console.WriteLine("курс {0} открыт", value);
                    }
                    OnPropertyChanged(() => OpenedCourse);
                }
                if (supressCourseClosing)
                {
                    supressCourseClosing = false;
                    OnPropertyChanged(() => OpenedCourse);
                }
            }
        }

        public AppointmentViewModel OpenedAppointment
        {
            get
            {
                return _openedApp;
            }
            private set
            {
                if (_openedApp != value)
                {
                    if (_openedApp != null)
                    {
                        OnAppointmentClosed(_openedApp);
                        Console.WriteLine("осмотр {0} закрыт", _openedApp);
                    }

                    _openedApp = value;

                    if (value != null)
                    {
                        OnAppointmentOpened(value);
                        Console.WriteLine("осмотр {0} открыт", value);
                    }

                    OnPropertyChanged(() => OpenedAppointment);
                    OpenedCourse.OnOpenedAppointmentChanged();
                }
            }
        }

        public HealthRecordViewModel OpenedHealthRecord
        {
            get
            {
                return _openedHr;
            }
            private set
            {
                if (_openedHr != value)
                {
                    _openedHr = value;
                    OnPropertyChanged(() => OpenedHealthRecord);
                }
            }
        }

        public void ClosePatient()
        {
            OpenedPatient = null;
        }

        public void OpenPatient(PatientViewModel patient)
        {
            OpenedPatient = patient;
        }

        /// <summary>
        /// Открывает последнюю за час встречу в последнем курсе. 
        /// Создает новую, если такой нет, в последнем курсе.
        /// Создает курс, если нет ни одного курса.
        /// </summary>
        /// <param name="patient"></param>
        public void OpenLastAppointment(PatientViewModel patient)
        {
            OpenPatient(patient);

            // последний курс или новый, если курсов нет
            var lastCourse = patient.CoursesManager.Courses.FirstOrDefault();
            if (lastCourse == null)
            {
                patient.CurrentDoctor.StartCourse(patient);
            }
            else
            {
                OpenedCourse = lastCourse;
            }

            // последняя встреча в течение часа или новая
            var lastApp = OpenedCourse.LastAppointment;
            if (DateTime.UtcNow - lastApp.DateTime > TimeSpan.FromHours(1))
            {
                OpenedCourse.AddAppointment();
            }
            else
            {
                OpenedAppointment = lastApp;
            }
        }

        internal void OpenHr(HealthRecord hr)
        {
            Contract.Requires(OpenedPatient.patient == hr.Appointment.Course.Patient);

            var course = OpenedPatient.CoursesManager.Courses.Where(x => x.course == hr.Appointment.Course).First();
            OpenedCourse = course;
            var app = course.Appointments.Where(x => x.appointment == hr.Appointment).First();
            OpenedAppointment = app;
            var hrVM = app.HealthRecords.Where(x => x.healthRecord == hr).First();
            app.SelectedHealthRecord = hrVM;
        }

        public void UnsubscribeSelectedHr()
        {
            if (OpenedCourse != null &&
                OpenedAppointment != null &&
                OpenedAppointment.SelectedHealthRecord != null)
            {
                OpenedAppointment.SelectedHealthRecord.UnsubscribeCheckedChanges();
            }
        }

        private void OnPatientOpened(PatientViewModel patient)
        {
            patient.Subscribe();
            patient.CurrentDoctor = doctor;
            patient.PatientViewer = this;

            CourseViewModel course;
            if (!patCourseMap.TryGetValue(patient, out course))
            {
                // пациент открыт первый раз
                OpenedCourse = patient.CoursesManager.Courses.FirstOrDefault();
            }
            else
            {
                // пацинет открыт повторно
                OpenedCourse = course;
            }

            if (FastAddingMode && !(patient is UnsavedPatientViewModel))
            {
                OpenLastAppointment(patient);
            }

            patient.CoursesManager.Courses.CollectionChanged += Courses_CollectionChanged;
        }

        private void OnPatientClosed(PatientViewModel patient)
        {
            patient.Unsubscribe();
            patient.Editable.Commit();

            patient.CoursesManager.Courses.CollectionChanged -= Courses_CollectionChanged;
            OpenedCourse = null;
        }
        private void OnCourseOpened(CourseViewModel course)
        {
            course.Appointments.CollectionChanged += Appointments_CollectionChanged;

            // map opened course to patient
            if (!patCourseMap.ContainsKey(OpenedPatient))
                patCourseMap.Add(OpenedPatient, course);
            else
                patCourseMap[OpenedPatient] = course;

            AppointmentViewModel app;
            if (!courseAppMap.TryGetValue(course, out app))
            {
                // курс открыт первый раз
                OpenedAppointment = OpenedCourse.LastAppointment;

                // для синхронизации c OpenedAppointmentWithAddNew
                OpenedCourse.OpenedAppointmentGetter = new Func<AppointmentViewModel>(() => OpenedAppointment);
                OpenedCourse.OpenedAppointmentSetter = new Action<AppointmentViewModel>((a) => { OpenedAppointment = a; });
            }
            else
            {
                OpenedAppointment = app;
            }
        }

        private void OnCourseClosed(CourseViewModel course)
        {
            course.Editable.Commit();

            course.Appointments.CollectionChanged -= Appointments_CollectionChanged;
        }

        private void OnAppointmentOpened(AppointmentViewModel app)
        {
            // map opened app to course
            if (!courseAppMap.ContainsKey(OpenedCourse))
                courseAppMap.Add(OpenedCourse, app);
            else
                courseAppMap[OpenedCourse] = app;
        }

        private void OnAppointmentClosed(AppointmentViewModel app)
        {
        }

        private void Courses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // при добавлении курса открываем его
                OpenedCourse = (CourseViewModel)e.NewItems[e.NewItems.Count - 1];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении курса открываем курс рядом с удаленным
                var i = e.OldStartingIndex;
                if (OpenedPatient.CoursesManager.Courses.Count <= i)
                    i--;
                if (OpenedPatient.CoursesManager.Courses.Count > 0)
                {
                    OpenedCourse = OpenedPatient.CoursesManager.Courses[i];
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                supressCourseClosing = true;
            }
        }

        private void Appointments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // при добавлении осмотра открываем его
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                OpenedAppointment = (AppointmentViewModel)e.NewItems[e.NewItems.Count - 1];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (OpenedAppointment == null && OpenedCourse.Appointments.Count > 0)
                {
                    OpenedAppointment = OpenedCourse.LastAppointment;
                }
            }
        }
    }
}