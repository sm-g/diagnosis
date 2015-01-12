using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CardViewModel : ScreenBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CardViewModel));
        private static PatientViewer viewer; // static to hold history
        private HrListViewModel _hrList;
        private HeaderViewModel _header;
        private HrEditorViewModel _hrEditor;
        private NavigatorViewModel _navigator;

        private bool editorWasOpened;
        private Saver saver;
        private EventMessageHandler handler;

        public CardViewModel(bool resetHistory = false)
        {
            if (resetHistory || viewer == null)
                viewer = new PatientViewer();

            saver = new Saver(Session);
            _navigator = new NavigatorViewModel(viewer);
            _hrEditor = new HrEditorViewModel(Session);

            Navigator.Navigating += (s, e) =>
            {
                // сначала создаем HrList, чтобы hrManager подписался на добавление записей первым, иначе HrList.SelectHealthRecord нечего выделять
                ShowHrsList(e.holder);
            };
            Navigator.CurrentChanged += (s, e) =>
            {
                // add to history
                HrEditor.Unload(); // закрываем редактор при смене активной сущности

                var holder = e.holder;

                ShowHrsList(holder);
                ShowHeader(holder);
                Title = MakeTitle();
            };

            HrEditor.Unloaded += (s, e) =>
            {
                // сохраняем запись при закрытии редактора
                var hr = e.entity as HealthRecord;
                saver.SaveHealthRecord(hr);

                // переходим к спсику записей
                HrList.IsFocused = true;
            };

            viewer.OpenedChanged += viewer_OpenedChanged;

            handler = this.Subscribe(Event.DeleteHolder, (e) =>
            {
                var holder = e.GetValue<IHrsHolder>(MessageKeys.Holder);

                // убрать из результатов поиска (или проверять при открытии, удален ли)
                viewer.RemoveFromHistory(holder);

                if (holder is Patient)
                {
                    Navigator.Remove(holder as Patient);
                    saver.Delete(holder);
                    if (Navigator.TopCardItems.Count == 0)
                        OnLastItemRemoved();
                    return;
                }
                else if (holder is Course)
                {
                    var course = holder as Course;
                    course.Patient.RemoveCourse(course);
                    saver.SaveAll(viewer.OpenedPatient); // сохраняем на случай, если удаление при открытом пациенте
                }
                else if (holder is Appointment)
                {
                    var app = holder as Appointment;
                    app.Course.RemoveAppointment(app);
                    saver.SaveAll(viewer.OpenedPatient);
                }
            });
        }

        public CardViewModel(object entity, bool resetHistory = false)
            : this(resetHistory)
        {
            Open(entity);
        }

        /// <summary>
        /// For XAML
        /// </summary>
        [Obsolete]
        public CardViewModel() { }

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

        public HrEditorViewModel HrEditor { get { return _hrEditor; } }

        public NavigatorViewModel Navigator { get { return _navigator; } }

        public HeaderViewModel Header
        {
            get
            {
                return _header;
            }
            set
            {
                if (_header != value)
                {
                    if (_header != null)
                        _header.Dispose();
                    _header = value;
                    OnPropertyChanged(() => Header);
                }
            }
        }

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

        public RelayCommand ToggleEditorCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ToogleHrEditor();
                }, () => HrList.SelectedHealthRecord != null);
            }
        }

        /// <summary>
        /// Открывает/закрывает редактор для выбранной в списке записи.
        /// </summary>
        public void ToogleHrEditor()
        {
            Contract.Ensures(HrList.SelectedHealthRecord == null ||
                HrEditor.HasHealthRecord != Contract.OldValue(HrEditor.HasHealthRecord));

            if (HrList.SelectedHealthRecord == null)
                return;

            // logger.DebugFormat("toggle hr editor from {0}", HrEditor.HasHealthRecord);
            if (HrEditor.HasHealthRecord)
            {
                Contract.Assume(HrEditor.HealthRecord.healthRecord == HrList.SelectedHealthRecord.healthRecord);
                HrEditor.Unload();
            }
            else
            {
                HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
            }
        }
        /// <summary>
        /// Открывает редактор для выбранной в списке записи и переводит фокус на него.
        /// </summary>
        public void FocusHrEditor()
        {
            //logger.DebugFormat("FocusHrEditor to {0}", HrList.SelectedHealthRecord);

            if (HrList.SelectedHealthRecord == null)
                return;

            HrEditor.Load(HrList.SelectedHealthRecord.healthRecord);
            HrEditor.IsFocused = true;
        }

        public void ResetHistory()
        {
            viewer = new PatientViewer();
        }
        /// <summary>
        /// Открывает держателя или выделяет записи. 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="lastAppOrCourse"></param>
        internal void Open(object parameter, bool lastAppOrCourse = false)
        {
            Contract.Requires(parameter != null);
            logger.DebugFormat("open {0}", parameter);

            IHrsHolder holder;
            if (parameter is IHrsHolder)
            {
                holder = Session.Unproxy(parameter as IHrsHolder);

                if (lastAppOrCourse)
                    viewer.AutoOpen = true;

                Navigator.NavigateTo(holder);

                if (lastAppOrCourse)
                    viewer.AutoOpen = false;
            }
            else if (parameter is HealthRecord)
            {
                var hr = parameter as HealthRecord;
                Navigator.Add(hr.GetPatient());
                holder = Session.Unproxy(hr.Holder as IHrsHolder);
                Navigator.NavigateTo(holder);
                HrList.SelectHealthRecord(hr);
            }
            else if (parameter is IEnumerable<HealthRecord>)
            {
                var hrs = parameter as IEnumerable<HealthRecord>;
                var first = hrs.FirstOrDefault();
                if (first != null)
                {
                    Navigator.Add(first.GetPatient());
                    holder = Session.Unproxy(first.Holder as IHrsHolder);
                    Navigator.NavigateTo(holder);
                    HrList.SelectHealthRecords(hrs);
                }
            }
        }

        private string MakeTitle()
        {
            if (Navigator.Current == null)
                return "";
            string delim = " — ";
            string result = string.Format("{0}", NameFormatter.GetShortName(viewer.OpenedPatient));

            var holder = Navigator.Current.Holder;

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
                HrList = new HrListViewModel(holder, (hr, hr2) =>
                {
                    hr2.Hios.Sync(Session, (w) => HrEditor.SyncTransientWord(w));

                    if (hr2.CategoryId != null)
                    {
                        hr.Category = Session.Get<HrCategory>(hr2.CategoryId.Value);
                    }
                    hr.FromYear = hr2.FromYear;
                    hr.FromMonth = hr2.FromMonth;
                    hr.FromDay = hr2.FromDay;
                    hr.Unit = hr2.Unit;
                    hr.SetItems(hr2.Hios);
                }, (hios) =>
                {
                    hios.Sync(Session, (w) => HrEditor.SyncTransientWord(w));
                });
                HrList.PropertyChanged += HrList_PropertyChanged;
                HrList.SaveNeeded += (s, e) =>
                {
                    logger.DebugFormat("SaveNeeded for hrs: {0}", e.list == null ? "All" : e.list.Count.ToString());

                    if (e.list == null)
                    {
                        // новые записи — вставка/дроп записей/тегов на список
                        // смена порядка — дроп записей

                        // ставим порядок
                        for (int i = 0; i < HrList.HealthRecords.Count; i++)
                        {
                            HrList.HealthRecords[i].healthRecord.Ord = i;
                        }
                        // сохраняем все записи кроме открытой в редакторе
                        saver.Save(HrList.HealthRecords
                            .Select(vm => vm.healthRecord)
                            .Except(HrEditor.HasHealthRecord ? HrEditor.HealthRecord.healthRecord.ToEnumerable() : Enumerable.Empty<HealthRecord>())
                            .ToArray());
                    }
                    else
                    {
                        // вставка/дроп тегов в записи
                        // изменение видимости (IsDeleted)

                        saver.Save(e.list.ToArray());
                    }
                };
                HrList.HealthRecords.CollectionChanged += HrList_HealthRecords_CollectionChanged;
            }
        }

        private void ShowHeader(IHrsHolder holder)
        {
            if (Header != null)
            {
                if (Header.Holder == holder)
                    return;

                Header.Dispose();
            }

            if (holder != null)
            {
                Header = new HeaderViewModel(holder);
            }
        }

        private void HrList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedHealthRecord")
            {
                if (HrList.SelectedHealthRecord != null)
                {
                    editorWasOpened = HrEditor.HasHealthRecord;
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
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                // порядок
            }
        }

        private void viewer_OpenedChanged(object sender, PatientViewer.OpeningEventArgs e)
        {
            var holder = e.entity as IHrsHolder;

            if (e.action == PatientViewer.OpeningAction.Close)
            {
                holder.HealthRecordsChanged -= HrsHolder_HealthRecordsChanged;

                // сохраняем пациента при закрытии чего-либо (переход вверх без закрытия не сохраняет)
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
                if (HrList.AddingHrByCommnd)
                {
                    // редактируем добавленную запись
                    var hr = (HealthRecord)e.NewItems[0];
                    HrList.SelectHealthRecord(hr);
                    HrEditor.Load(hr);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // удаляем записи в бд
                saver.Delete(e.OldItems.Cast<HealthRecord>().ToArray());
            }
        }

        protected virtual void OnLastItemRemoved()
        {
            var h = LastItemRemoved;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Header.Dispose();
                    HrEditor.Dispose();
                    HrList.Dispose(); // удаляются все записи

                    viewer.CloseAll(); // сохраняется пациент если открыт
                    viewer.OpenedChanged -= viewer_OpenedChanged;
                    Navigator.Dispose();
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