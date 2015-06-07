using EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class EventAggragatorExtensions
    {
        // для каждого event новый объект как eventSource

        public static EventMessageHandler Subscribe(this Event @event, Action<EventMessage> handler)
        {
            return Subscribe((int)@event, handler);
        }

        public static EventMessage Send(this Event @event, params KeyValuePair<string, object>[] parameters)
        {
            return Send((int)@event, parameters);
        }

        public static EventMessageHandler Subscribe(this int @event, Action<EventMessage> handler)
        {
            return new object().Subscribe(@event, handler);
        }

        public static EventMessage Send(this int @event, params KeyValuePair<string, object>[] parameters)
        {
            return new object().Send(@event, parameters);
        }

        public static EventMessageHandler Subscribe(this object obj, Event @event, Action<EventMessage> handler)
        {
            return new object().Subscribe((int)@event, handler);
        }

        public static EventMessage Send(this object obj, Event @event, params KeyValuePair<string, object>[] parameters)
        {
            return new object().Send((int)@event, parameters);
        }

        public static KeyValuePair<string, object>[] AsParams(this object obj, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, obj) };
        }

        /// <summary>
        /// Массив как параметр.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KeyValuePair<string, object>[] AsParams(this object[] array, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, array) };
        }

        /// <summary>
        /// Несколько объектов как параметры.
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static KeyValuePair<string, object>[] AsParams(this object[] objects, params string[] keys)
        {
            var result = objects.Zip(keys, (obj, key) => new KeyValuePair<string, object>(key, obj));
            return result.ToArray();
        }
    }
}
