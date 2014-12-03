using EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Common
{
    /// <summary>
    /// Ключи событий для EventAggregator
    /// </summary>
    public static class MessageKeys
    {
        public const string Word = "word";
        public const string Words = "words";
        public const string Patient = "patient";
        public const string Doctor = "doctor";
        public const string Diagnosis = "diagnosis";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";
        public const string HealthRecords = "healthRecords";
        public const string Category = "category";
        public const string Boolean = "bool";
        public const string Settings = "settings";
        public const string UndoOverlay = "undooverlay";
        public const string Type = "type";
        public const string Holder = "holder";
        public const string Dialog = "dialog";
        public const string HrItemObjects = "entity";
    }

    public enum Events
    {
        // for ScreenSwitcher
        OpenPatient,
        OpenCourse,
        OpenAppointment,
        OpenHealthRecord,
        EditHealthRecord,
        OpenHolder,

        // windows
        CreatePatient,
        EditPatient,
        EditHolder,
        OpenSettings,
        OpenDialog,

        SettingsSaved,

        // for Search

        SendToSearch,

        ShowUndoOverlay,
        HideOverlay,
        Shutdown,
        EntityDeleted,
        WordPersisted
    }

    public static class EventAggragatorExtensions
    {
        public static EventMessageHandler Subscribe(this object obj, Events @event, Action<EventMessage> handler)
        {
            return obj.Subscribe((int)@event, handler);
        }

        public static EventMessage Send(this object obj, Events @event, params KeyValuePair<string, object>[] parameters)
        {
            return obj.Send((int)@event, parameters);
        }

        public static KeyValuePair<string, object>[] AsParams(this object obj, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, obj) };
        }

        /// <summary>
        /// Массив как параметр.
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KeyValuePair<string, object>[] AsParams(this object[] objs, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, objs) };
        }

        /// <summary>
        /// Несколько объектов как параметры.
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static KeyValuePair<string, object>[] AsParams(this object[] objs, params string[] keys)
        {
            var result = objs.Zip(keys, (obj, key) => new KeyValuePair<string, object>(key, obj));
            return result.ToArray();
        }
    }
}