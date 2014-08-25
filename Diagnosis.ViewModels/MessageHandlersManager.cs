using EventAggregator;
using System.Collections.Generic;

namespace Diagnosis.App.Messaging
{
    internal class MessageHandlersManager
    {
        private List<EventMessageHandler> list = new List<EventMessageHandler>();

        public MessageHandlersManager()
        {
        }

        public MessageHandlersManager(IEnumerable<EventMessageHandler> handlers)
        {
            list.AddRange(handlers);
        }

        public MessageHandlersManager(EventMessageHandler handler)
        {
            list.Add(handler);
        }

        public void Add(EventMessageHandler handler)
        {
            list.Add(handler);
        }

        public void Add(IEnumerable<EventMessageHandler> handlers)
        {
            foreach (var item in handlers)
            {
                Add(item);
            }
        }

        public void Remove(EventMessageHandler handler)
        {
            list.Remove(handler);
        }

        public void Dispose(EventMessageHandler handler)
        {
            handler.Dispose();
            Remove(handler);
        }

        public void Dispose()
        {
            for (int i = 0; i < list.Count; i++)
            {
                Dispose(list[i]);
            }
        }
    }
}