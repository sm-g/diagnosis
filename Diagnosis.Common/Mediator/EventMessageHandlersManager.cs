﻿using System.Collections.Generic;

namespace EventAggregator
{
    /// <summary>
    /// Tracks list of EventAggragator EventMessageHandler's. Call Dispose to unsubscribe from all events.
    /// </summary>
    public class EventMessageHandlersManager
    {
        private List<EventMessageHandler> list = new List<EventMessageHandler>();

        public EventMessageHandlersManager()
        {
        }

        public EventMessageHandlersManager(IEnumerable<EventMessageHandler> handlers)
        {
            Add(handlers);
        }

        public EventMessageHandlersManager(EventMessageHandler handler)
        {
            Add(handler);
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