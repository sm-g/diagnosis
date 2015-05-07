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
        public static string Word = "word";
        public static string Patient = "patient";
        public static string Doctor = "doctor";
        public static string User = "user";
        public static string Diagnosis = "diagnosis";
        public static string Course = "course";
        public static string Appointment = "appointment";
        public static string HealthRecord = "healthRecord";
        public static string HealthRecords = "healthRecords";
        public static string Category = "category";
        public static string Boolean = "bool";
        public static string Settings = "settings";
        public static string UndoDoActions = "undooverlay";
        public static string Type = "type";
        public static string Holder = "holder";
        public static string Dialog = "dialog";
        public static string Chios = "entity";
        public static string Window = "window";
        public static string Uom = "uom";
        public static string String = "string";
        public static string Name = "name";
        public static string Value = "value";
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