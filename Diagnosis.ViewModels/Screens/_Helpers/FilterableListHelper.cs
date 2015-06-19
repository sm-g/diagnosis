using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    internal class FilterableListHelper<T, TVm> : IDisposable
        where TVm : CheckableBase
        where T : class
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

        public void AddAfterEntitySavedAction(Action afterEvent)
        {
            var handler = this.Subscribe(Event.EntitySaved, (e) =>
            {
                var entity = e.GetValue<IEntity>(MessageKeys.Entity);
                if (entity is T)
                {
                    SelectVmWithEntity(entity as T);
                    afterEvent();
                }
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