using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public interface IResultItem
    {

    }

    public abstract class AbstractSearchResultViewModel : ViewModelBase
    {
        private EventMessageHandler handler;

        public AbstractSearchResultViewModel()
        {
            handler = this.Subscribe(Event.DeleteHolder, (e) =>
            {
                // убираем удаленного холдера из результатов
                var h = e.GetValue<IHrsHolder>(MessageKeys.Holder);
                RemoveDeleted(h);
            });
        }

        public ObservableCollection<IResultItem> Patients { get; protected set; }

        public StatisticBase Statistic { get; protected set; }

        /// <summary>
        /// Источник запроса, по которому получен результат.
        /// </summary>
        public abstract object QuerySource { get; }

        public RelayCommand ExportCommand
        {
            get
            {
                return new RelayCommand(Export);
            }
        }

        protected abstract void Export();

        protected abstract void RemoveDeleted(IHrsHolder h);

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                    Patients.OfType<IDisposable>().ForAll(x => x.Dispose());
                    Patients = null;
                    Statistic.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}