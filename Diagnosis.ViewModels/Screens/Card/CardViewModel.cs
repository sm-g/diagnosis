using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CardViewModel : ScreenBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CardViewModel));
        private static PatientViewer viewer; // static to hold history
        private HrListViewModel _hrList;
        private HrEditorViewModel _hrEditor;
        private bool editorWasOpened;

        private Saver saver;
        private EventMessageHandler handler;
        private NavigatorViewModel navigator;

        public CardViewModel(object entity, bool resetHistory = false)
        {
            if (resetHistory || viewer == null)
                viewer = new PatientViewer();

            saver = new Saver(Session);
            navigator = new NavigatorViewModel(viewer);
            HrEditor = new HrEditorViewModel(Session);

            navigator.Navigating += (s, e) =>
            {
                // сначала создаем HrList, чтобы hrManager подписался на добавление записей первым, иначе HrList.SelectHealthRecord нечего выделять
                ShowHrsList(e.holder);
            };
            navigator.CurrentChanged += (s, e) =>
            {
                // add to history
                HrEditor.Unload(); // закрываем редактор при смене активной сущности

                var holder = e.holder;

                ShowHrsList(holder);
                Title = MakeTitle();
            };
            navigator.TopCardItems.CollectionChanged += (s, e) =>
            {
                if (navigator.TopCardItems.Count == 0)
                    OnPatientClosed();
            };

            HrEditor.Unloaded += (s, e) =>
            {
                // сохраняем запись при закрытии редактора
                var hr = e.entity as HealthRecord;
                saver.SaveHealthRecord(hr);
            };

            viewer.OpenedChanged += viewer_OpenedChanged;

            handler = this.Subscribe(Events.EntityDeleted, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);

                // убрать из результатов поиска (или проверять при открытии, удален ли)

                if (holder is Patient)
                {
                    saver.Delete(holder);
                }
                else if (holder is Course)
                {
                    var course = holder as Course;
                    course.Patient.RemoveCourse(course);
                }
                else if (holder is Appointment)
                {
                    var app = holder as Appointment;
                    app.Course.RemoveAppointment(app);
                }

                viewer.Close(holder);
                viewer.RemoveFromHistory(holder);
            });

            Open(entity);
        }

        /// <summary>
        /// For XAML-editor only
        /// </summary>
        [Obsolete]
        public CardViewModel()
        { }

        /// <summary>
        /// После удаления всех элементов, карточка пуста.
        /// </summary>
        public event EventHandler LastItemRemoved;

        public HrListViewModel HrList
        {
            get
            {
                return _hrList;
            }
            set
            {
                if (_hrList != value)
                {
                    _hrList = value;
                    OnPropertyChanged(() => HrList);
                }
            }
        }

        public HrEditorViewModel HrEditor
        {
            get
            {
                return _hrEditor;
            }
            private set
            {
                if (_hrEditor != value)
                {
                    _hrEditor = value;
                    OnPropertyChanged(() => HrEditor);
                }
            }
        }

        public NavigatorViewModel Navigator { get { return navigator; } }

        public RelayCommand StartCourseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    AuthorityController.CurrentDoctor.StartCourse(viewer.OpenedPatient);
                });
            }
        }

        public RelayCommand AddAppointmentCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    viewer.OpenedCourse.AddAppointment(AuthorityController.CurrentDoctor);
                },
                () => viewer.OpenedCourse != null && viewer.OpenedCourse.End == null);
            }
        }

        /// <summary>
        /// Переключает редактор для открытой записи.
        /// </summary>
        public void ToogleHrEditor()
        {
            if (HrEditor.IsActive && HrEditor.HealthRecord.healthRecord == HrList.SelectedHealthRecord.healthRecord)
            {
                HrEditor.Unload();
                // редактор записей после смены осмотра всегда закрыт
                editorWasOpened = false;
            }
            else
            {
                HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
            }
        }

        public void ResetHistory()
        {
            viewer = new PatientViewer();
        }

        internal void Open(object parameter)
        {
            Contract.Requires(parameter != null);
            logger.DebugFormat("open {0}", parameter);

            IHrsHolder holder;
            if (parameter is IHrsHolder)
            {
                holder = Session.Unproxy(parameter as IHrsHolder);
                navigator.NavigateTo(holder);
            }
            else
            {
                var hr = parameter as HealthRecord;
                holder = Session.Unproxy(hr.Holder as IHrsHolder);
                navigator.NavigateTo(holder);
                HrList.SelectHealthRecord(hr);
            }
        }

        private string MakeTitle()
        {
            string delim = " — ";
            string result = string.Format("{0} {1}", viewer.OpenedPatient.Label, NameFormatter.GetShortName(viewer.OpenedPatient));

            var holder = navigator.Current.Holder;

            if (holder is Course)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedCourse.Start, viewer.OpenedCourse.End);
            }
            else if (holder is Appointment)
            {
                result += delim + "курс " + DateFormatter.GetIntervalString(viewer.OpenedCourse.Start, viewer.OpenedCourse.End);
                result += delim + "осмотр " + DateFormatter.GetDateString(viewer.OpenedAppointment.DateAndTime);
            }
            return result;
        }

        /// <summary>
        /// Показвает записи активной сущности.
        /// </summary>
        /// <param name="holder"></param>
        private void ShowHrsList(IHrsHolder holder)
        {
            if (HrList != null)
            {
                if (HrList.holder == holder)
                    return; // список может быть уже создан

                HrList.Dispose(); // delete hrs here
            }

            if (holder != null)
            {
                HrList = new HrListViewModel(holder);
                HrList.PropertyChanged += HrList_PropertyChanged;
                HrList.HealthRecords.CollectionChanged += HrList_HealthRecords_CollectionChanged;
            }
        }

        private void HrList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedHealthRecord")
            {
                if (HrList.SelectedHealthRecord != null)
                {
                    editorWasOpened = HrEditor.IsActive;
                    if (editorWasOpened)
                    {
                        HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
                    }
                }
                else
                {
                    HrEditor.Unload();
                }
            }
        }

        private void HrList_HealthRecords_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Catch hr.isDeleted = true
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Count > 0)
                {
                    saver.SaveHealthRecord((e.OldItems[0] as ShortHealthRecordViewModel).healthRecord);
                }
            }
        }

        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            var holder = e.entity as IHrsHolder;

            if (e.action == PatientViewer.OpeningAction.Close)
            {
                holder.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;

                // сохраняем сохраняем пациента при закрытии элемента (переход вверх без зарктыия не сохраняет)
                saver.SaveAll(viewer.OpenedPatient, deleteEmptyHrs: true);
            }
            else
            {
                holder.HealthRecordsChanged += HrsHolder_HealthRecordsChanged;
            }
        }

        private void HrsHolder_HealthRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // редактируем добавленную запись
                var hr = (HealthRecord)e.NewItems[0];
                HrList.SelectHealthRecord(hr);
                HrEditor.Load(hr);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // удаляем записи в бд
                saver.SaveAll(viewer.OpenedPatient);
            }
        }

        protected virtual void OnPatientClosed()
        {
            var h = LastItemRemoved;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Contract.Assume(viewer.OpenedPatient != null);
            }

            try
            {
                if (disposing)
                {
                    HrEditor.Dispose();
                    HrList.Dispose(); // удаляются все записи

                    viewer.Close(viewer.OpenedPatient); // сохраняется пациент
                    viewer.OpenedChanged -= viewer_OpenedChanged;
                    navigator.Dispose();
                    handler.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}