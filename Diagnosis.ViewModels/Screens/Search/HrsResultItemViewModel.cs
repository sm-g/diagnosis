using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{

    public class HrsResultItemViewModel : HierarchicalBase<HrsResultItemViewModel>, IHrsHolderKeeper, IResultItem
    {
        private readonly IHrsHolder holder;

        private HrsResultItemViewModel(IHrsHolder holder, IEnumerable<HealthRecord> foundHrs = null)
        {
            this.holder = holder;
            // автообновление результатов поиска - сейчас только удаляется запись/холдер, но не статистика
            Holder.HealthRecordsChanged += Holder_HealthRecordsChanged;

            HealthRecords = new ObservableCollection<HealthRecord>(holder.HealthRecords);
            if (foundHrs != null)
                FoundHealthRecords = new ObservableCollection<HealthRecord>(foundHrs);
            else
                FoundHealthRecords = new ObservableCollection<HealthRecord>();

            FoundHealthRecords.CollectionChanged += (s, e) =>
            {
                if (FoundHealthRecords.Count == 0 && this.IsTerminal)
                    // когда удалены все записи, по которым был найден холдер
                    this.Remove();
            };
        }

        /// <summary>
        /// Создает результаты поиска по найденным записям.
        /// </summary>
        /// <param name="hrs"></param>
        /// <returns></returns>
        public static IEnumerable<HrsResultItemViewModel> MakeFrom(IEnumerable<HealthRecord> hrs)
        {
            return MakeTreeResults1(hrs);
        }

        public IHrsHolder Holder { get { return holder; } }

        /// <summary>
        /// Записи, подходящие под поиковый запрос
        /// </summary>
        public ObservableCollection<HealthRecord> FoundHealthRecords { get; private set; }

        /// <summary>
        /// Все записи держателя
        /// </summary>
        public ObservableCollection<HealthRecord> HealthRecords { get; private set; }

        /// <summary>
        /// Открывает первую найденную запись или держателя.
        /// </summary>
        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        if (FoundHealthRecords.Count > 0)
                            this.Send(Event.OpenHealthRecords, FoundHealthRecords.AsParams(MessageKeys.HealthRecords));
                        else
                            this.Send(Event.OpenHolder, Holder.AsParams(MessageKeys.Holder));
                    });
            }
        }

        private static IEnumerable<HrsResultItemViewModel> MakeTreeResults1(IEnumerable<HealthRecord> hrs)
        {
            // результаты для каждой записи с держателем записи
            var appRvms = CreateResultsWithHolder(hrs, (hr) => hr.Appointment);
            var courseRvms = CreateResultsWithHolder(hrs, (hr) => hr.Course);
            var patRvms = CreateResultsWithHolder(hrs, (hr) => hr.Patient);

            // группируем и добавляем нужные родительские держатели, снизу вверх
            // осмотры в курс
            foreach (var appRvm in appRvms)
            {
                var course = ((appRvm.Holder) as Appointment).Course;
                var cRvm = courseRvms.Where(rvm => (Course)rvm.Holder == course).FirstOrDefault();
                if (cRvm == null)
                {
                    cRvm = new HrsResultItemViewModel(course);
                    courseRvms.Add(cRvm);
                }
                cRvm.Children.Add(appRvm);
            }

            // курсы в пациентов
            foreach (var courseRvm in courseRvms)
            {
                var patient = ((courseRvm.Holder) as Course).Patient;
                var pRvm = patRvms.Where(rvm => (Patient)rvm.Holder == patient).FirstOrDefault();
                if (pRvm == null)
                {
                    pRvm = new HrsResultItemViewModel(patient);
                    patRvms.Add(pRvm);
                }
                pRvm.Children.Add(courseRvm);
            }
            return patRvms;
        }

        private static IEnumerable<HrsResultItemViewModel> MakeTreeResults2(IEnumerable<HealthRecord> hrs)
        {
            // сверху вниз, сначала все записи в результатах с пациентом
            var pRvms = CreateResultsWithHolder(hrs, (hr) => hr.GetPatient());

            foreach (var pRvm in pRvms)
            {
                // записи курсов перемещаем в новые результаты
                var cRvms = CreateResultsWithHolder(pRvm.FoundHealthRecords, (hr) => hr.GetCourse());
                foreach (var cRvm in cRvms)
                {
                    cRvm.FoundHealthRecords.ForAll(hr => pRvm.FoundHealthRecords.Remove(hr));
                    pRvm.Children.Add(cRvm);

                    // перермещаем записи осмотров
                    var aRvms = CreateResultsWithHolder(cRvm.FoundHealthRecords, (hr) => hr.Appointment);
                    foreach (var aRvm in aRvms)
                    {
                        aRvm.FoundHealthRecords.ForAll(hr => cRvm.FoundHealthRecords.Remove(hr));
                        cRvm.Children.Add(aRvm);
                    }
                }
            }
            return pRvms;
        }

        /// <summary>
        /// Создает результаты поиска для найденных записей, группируя по держателю записей.
        /// </summary>
        private static List<HrsResultItemViewModel> CreateResultsWithHolder(IEnumerable<HealthRecord> hrs, Func<HealthRecord, IHrsHolder> holderOf)
        {
            return (from hr in hrs
                    where holderOf(hr) != null
                    group hr by holderOf(hr) into g
                    orderby g.Key
                    select new HrsResultItemViewModel(g.Key, g)).ToList();
        }

        private void Holder_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var hr = e.OldItems[0] as HealthRecord;
                HealthRecords.Remove(hr);
                FoundHealthRecords.Remove(hr);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Holder.HealthRecordsChanged -= Holder_HealthRecordsChanged;
            }

            base.Dispose(disposing);
        }
    }
}