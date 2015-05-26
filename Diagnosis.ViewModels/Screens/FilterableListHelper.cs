using Diagnosis.Common;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    internal class FilterableListHelper<T, TVm> : IDisposable where TVm : CheckableBase
    {
        private EventMessageHandlersManager emhManager;
        private IFilterableList list;
        private Func<TVm, T> selector;

        public FilterableListHelper(IFilterableList list, Func<TVm, T> selector)
        {
            emhManager = new EventMessageHandlersManager();
            this.list = list;
            this.selector = selector;
        }

        public void AddSelectVmWithEntityOn(Event @event,
            string TMessageKey,
            Action afterEvent)
        {
            var handler = this.Subscribe(@event, (e) =>
            {
                var entity = e.GetValue<T>(TMessageKey);

                SelectVmWithEntity(entity);

                afterEvent();
            });

            emhManager.Add(handler);
        }

        private void SelectVmWithEntity(T entity)
        {
            list.Filter.Filter();

            var visibleVm = list.Items
                .Where(x => selector(x as TVm).Equals(entity))
                .FirstOrDefault();

            if (visibleVm != null)
                visibleVm.IsSelected = true;
        }

        public void Dispose()
        {
            emhManager.Dispose();
        }
    }
}