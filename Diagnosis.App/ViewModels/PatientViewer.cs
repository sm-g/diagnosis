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
    /// <summary>
    /// Обрабатывает действия при открытии и закрытии пациента, курса, осмотра, записи.
    /// </summary>
    public class PatientViewer : ViewModelBase
    {
        private readonly DoctorViewModel doctor;
        private PatientViewModel _openedPatient;
        private AppointmentViewModel _openedApp;
        private CourseViewModel _openedCourse;
        private HealthRecordViewModel _openedHr;
        private bool _fastAddingMode;

        // последние открытые
        private Dictionary<PatientViewModel, CourseViewModel> patCourseMap;
        private Dictionary<CourseViewModel, AppointmentViewModel> courseAppMap;
        private Dictionary<AppointmentViewModel, HealthRecordViewModel> appHrMap;
        private bool supressCourseClosing;

        public PatientViewer(DoctorViewModel doctor)
        {
            this.doctor = doctor;
            patCourseMap = new Dictionary<PatientViewModel, CourseViewModel>();
            courseAppMap = new Dictionary<CourseViewModel, AppointmentViewModel>();
            appHrMap = new Dictionary<AppointmentViewModel, HealthRecordViewModel>();
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
                        OnPatientClosed(_openedPatient, value);
                        Console.WriteLine("пациент {0} закрыт", _openedPatient);
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
                    OpenedPatient.OnOpenedCourseChanged();
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
                    if (_openedHr != null)
                    {
                        OnHrClosed(_openedHr, value);
                        Console.WriteLine("запись {0} закрыта", _openedHr);
                    }
                    _openedHr = value;
                    OpenedAppointment.OnOpenedHealthRecordChanged();

                    if (value != null)
                    {
                        OnHrOpened(value);
                        Console.WriteLine("запись {0} открыта", value);
                    }
                    else
                    {
                        Console.WriteLine("нет открытых записей");
                    }

                    OnPropertyChanged(() => OpenedHealthRecord);
                }
                else
                {
                    Console.WriteLine("same hr");
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
        /// Открывает последний за час осмотр в последнем курсе. 
        /// Создает новую, если такой нет, в последнем курсе.
        /// Создает курс, если нет ни одного курса.
        /// </summary>
        /// <param name="patient"></param>
        public void OpenLastAppointment(PatientViewModel patient)
        {
            OpenPatient(patient);

            // последний курс или новый, если курсов нет
            var lastCourse = patient.Courses.FirstOrDefault();
            if (lastCourse == null)
            {
                patient.CurrentDoctor.StartCourse(patient);
            }
            else
            {
                OpenedCourse = lastCourse;
            }

            // последний осмотр в течение часа или новая
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

            var course = OpenedPatient.Courses.Where(x => x.course == hr.Appointment.Course).First();
            OpenedCourse = course;
            var app = course.Appointments.Where(x => x.appointment == hr.Appointment).First();
            OpenedAppointment = app;
            var hrVM = app.HealthRecords.Where(x => x.healthRecord == hr).First();
            OpenedHealthRecord = hrVM;
        }

        private void OnPatientOpened(PatientViewModel patient)
        {
            patient.Courses.CollectionChanged += Courses_CollectionChanged;

            patient.Subscribe();
            patient.CurrentDoctor = doctor;

            CourseViewModel course;
            if (!patCourseMap.TryGetValue(patient, out course))
            {
                // пациент открыт первый раз
                OpenedCourse = patient.Courses.FirstOrDefault();

                // для синхронизации c SelectedCourse
                OpenedPatient.OpenedCourseGetter = new Func<CourseViewModel>(() => OpenedCourse);
                OpenedPatient.OpenedCourseSetter = new Action<CourseViewModel>((a) =>
                {
                    OpenedCourse = a;
                });
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
        }

        private void OnPatientClosed(PatientViewModel closing, PatientViewModel opening)
        {
            closing.Courses.CollectionChanged -= Courses_CollectionChanged;

            closing.Unsubscribe();
            closing.Editable.Commit();

            OpenedCourse = null;

            if (opening != null)
            {
                if (!(opening is UnsavedPatientViewModel))
                {
                    // сохраняем состояние редактора при смене пациента
                    opening.Editable.IsEditorActive = closing.Editable.IsEditorActive;
                }
            }
            else
            {
                if (OpenedHealthRecord != null)
                {
                    OpenedHealthRecord.UnsubscribeCheckedChanges();
                }
            }
        }
        private void OnCourseOpened(CourseViewModel course)
        {
            course.Appointments.CollectionChanged += Appointments_CollectionChanged;

            if (!patCourseMap.ContainsKey(OpenedPatient))
            {
                // пациент открыт первый раз
                patCourseMap.Add(OpenedPatient, course);
            }
            else
                patCourseMap[OpenedPatient] = course;

            AppointmentViewModel app;
            if (!courseAppMap.TryGetValue(course, out app))
            {
                // курс открыт первый раз
                if (OpenedCourse.Appointments.Count == 0)
                {
                    OpenedCourse.AddAppointment(); // новый курс — добавляем осмотр
                }

                OpenedAppointment = OpenedCourse.LastAppointment;

                // для синхронизации c SelectedAppointmentWithAddNew
                OpenedCourse.OpenedAppointmentGetter = new Func<AppointmentViewModel>(() => OpenedAppointment);
                OpenedCourse.OpenedAppointmentSetter = new Action<AppointmentViewModel>((a) =>
                {
                    OpenedAppointment = a;
                });
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
            OpenedAppointment = null;
        }

        private void OnAppointmentOpened(AppointmentViewModel app)
        {
            app.HealthRecords.CollectionChanged += HealthRecords_CollectionChanged;

            if (!courseAppMap.ContainsKey(OpenedCourse))
            {
                // курс открыт первый раз
                courseAppMap.Add(OpenedCourse, app);
            }
            else
            {
                courseAppMap[OpenedCourse] = app;
            }

            HealthRecordViewModel hrVm;
            if (!appHrMap.TryGetValue(app, out hrVm))
            {
                // осмотр открыт первый раз

                // для синхронизации c SelectedHealthRecord
                OpenedAppointment.OpenedHrGetter = new Func<HealthRecordViewModel>(() => OpenedHealthRecord);
                OpenedAppointment.OpenedHrSetter = new Action<HealthRecordViewModel>((a) => { OpenedHealthRecord = a; });

                if (OpenedAppointment.HealthRecords.Count == 0)
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
                OpenedHealthRecord.Editable.IsEditorActive = false;
            }
        }
        private void OnAppointmentClosed(AppointmentViewModel app)
        {
            app.HealthRecords.CollectionChanged -= HealthRecords_CollectionChanged;

            OpenedHealthRecord = null;
        }

        private void OnHrOpened(HealthRecordViewModel hr)
        {
            if (!appHrMap.ContainsKey(OpenedAppointment))
            {
                // осмотр открыт первый раз
                appHrMap.Add(OpenedAppointment, hr);
            }
            else
            {
                appHrMap[OpenedAppointment] = hr;
            }


            hr.SubscribeToCheckedChanges();
            hr.CheckInCurrent();
        }

        private void OnHrClosed(HealthRecordViewModel closing, HealthRecordViewModel opening)
        {
            // новая выбранная запись в том же приеме
            if (opening != null && closing.healthRecord.Appointment == opening.healthRecord.Appointment)
            {
                // оставляем редактор открытым при смене выбранной записи
                if (closing.Editable.IsEditorActive)
                {
                    opening.Editable.IsEditorActive = closing.Editable.IsEditorActive;
                }
            }
            closing.Editable.IsEditorActive = false;
            closing.Editable.Commit();
            closing.UnsubscribeCheckedChanges();
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
                if (OpenedPatient.Courses.Count <= i)
                    i--;
                if (OpenedPatient.Courses.Count > 0)
                {
                    OpenedCourse = OpenedPatient.Courses[i];
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
                OpenedAppointment = (AppointmentViewModel)e.NewItems[e.NewItems.Count - 1];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // при удалении осмотра открываем последний осмотр
                if (OpenedAppointment == null && OpenedCourse.Appointments.Count > 0)
                {
                    OpenedAppointment = OpenedCourse.LastAppointment;
                }
            }
        }

        void HealthRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // открываем добавленную запись на редактирование
                OpenedHealthRecord = (HealthRecordViewModel)e.NewItems[e.NewItems.Count - 1];
                OpenedHealthRecord.Editable.IsEditorActive = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {

            }
        }
    }
}