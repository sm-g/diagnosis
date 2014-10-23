using Diagnosis.Core;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Search
{
    public class HrHolderSearchResultViewModel : HierarchicalBase<HrHolderSearchResultViewModel>
    {
        private HrHolderSearchResultViewModel(IHrsHolder holder, IEnumerable<HealthRecord> hrs = null)
        {
            Holder = holder;
            // автообновление результатов поиска - сейчас только удаляются запись и добавляется, но не обновляется текст записи
            Holder.HealthRecordsChanged += Holder_HealthRecordsChanged;
            if (hrs != null)
            {
                FoundHealthRecords = new ObservableCollection<HealthRecord>(hrs);
            }
            else
            {
                FoundHealthRecords = new ObservableCollection<HealthRecord>();
            }
            HealthRecords = new ObservableCollection<HealthRecord>(holder.HealthRecords);
        }

        /// <summary>
        /// Создает результаты поиска по найденным записям.
        /// </summary>
        /// <param name="hrs"></param>
        /// <returns></returns>
        public static IEnumerable<HrHolderSearchResultViewModel> MakeFrom(IEnumerable<HealthRecord> hrs)
        {
            return MakeTreeResults1(hrs);
        }

        public IHrsHolder Holder { get; private set; }

        public ObservableCollection<HealthRecord> FoundHealthRecords { get; private set; }

        public ObservableCollection<HealthRecord> HealthRecords { get; private set; }

        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        if (FoundHealthRecords.Count > 0)
                            this.Send(Events.OpenHealthRecord, FoundHealthRecords.First().AsParams(MessageKeys.HealthRecord));
                        else
                            this.Send(Events.OpenHolder, Holder.AsParams(MessageKeys.Holder));
                    });
            }
        }

        private static IEnumerable<HrHolderSearchResultViewModel> MakeTreeResults1(IEnumerable<HealthRecord> hrs)
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
                    cRvm = new HrHolderSearchResultViewModel(course);
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
                    pRvm = new HrHolderSearchResultViewModel(patient);
                    patRvms.Add(pRvm);
                }
                pRvm.Children.Add(courseRvm);
            }
            return patRvms;
        }

        private static IEnumerable<HrHolderSearchResultViewModel> MakeTreeResults2(IEnumerable<HealthRecord> hrs)
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
        private static List<HrHolderSearchResultViewModel> CreateResultsWithHolder(IEnumerable<HealthRecord> hrs, Func<HealthRecord, IHrsHolder> holderOf)
        {
            return (from hr in hrs
                    where holderOf(hr) != null
                    group hr by holderOf(hr) into g
                    select new HrHolderSearchResultViewModel(g.Key, g)).ToList();
        }

        private void Holder_HealthRecordsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                HealthRecords.Add(e.NewItems[0] as HealthRecord);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                HealthRecords.Remove(e.OldItems[0] as HealthRecord);
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

    public static class HrExtensions
    {
        public static Patient GetPatient(this HealthRecord hr)
        {
            return hr.Patient ?? (hr.Course != null ? hr.Course.Patient : hr.Appointment.Course.Patient);
        }

        public static Course GetCourse(this HealthRecord hr)
        {
            return hr.Course ?? (hr.Appointment != null ? hr.Appointment.Course : null);
        }
    }
}