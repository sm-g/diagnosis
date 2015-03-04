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
        public const string Patient = "patient";
        public const string Doctor = "doctor";
        public const string User = "user";
        public const string Diagnosis = "diagnosis";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";
        public const string HealthRecords = "healthRecords";
        public const string Category = "category";
        public const string Boolean = "bool";
        public const string Settings = "settings";
        public const string UndoDoActions = "undooverlay";
        public const string Type = "type";
        public const string Holder = "holder";
        public const string Dialog = "dialog";
        public const string HrItemObjects = "entity";
        public const string Window = "window";
        public const string Uom = "uom";
        public const string String = "string";
        public const string Name = "name";
        public const string Value = "value";
    }

    public enum Event
    {
        // for ScreenSwitcher
        OpenPatient,
        OpenCourse,
        OpenAppointment,
        OpenHealthRecords,
        EditHealthRecord,
        OpenHolder,

        // windows
        EditDoctor,
        EditPatient,
        EditHolder,
        EditWord,
        EditUom,
        OpenSettings,
        OpenDialog,
        OpenWindow,

        // card logic
        SendToSearch,
        ShowUndoOverlay,
        HideOverlay,
        DeleteHolder,
        AddHr,

        // nhibernate
        WordPersisted,
        WordSaved,
        PatientSaved,
        DoctorSaved,
        UomSaved,
        SettingsSaved,

        // other
        ChangeTheme,
        ChangeFont,

        ShowHelp,

        // app
        PushToSettings,
        Shutdown,
    }

    public static class EventAggragatorExtensions
    {
        public static EventMessageHandler Subscribe(this object obj, Event @event, Action<EventMessage> handler)
        {
            return obj.Subscribe((int)@event, handler);
        }

        public static EventMessage Send(this object obj, Event @event, params KeyValuePair<string, object>[] parameters)
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