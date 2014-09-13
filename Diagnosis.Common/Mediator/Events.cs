using EventAggregator;
using System;
using System.Collections.Generic;

namespace Diagnosis.Core
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
        public const string Property = "property";
        public const string Course = "course";
        public const string Appointment = "appointment";
        public const string HealthRecord = "healthRecord";
        public const string HealthRecords = "healthRecords";
        public const string Category = "category";
        public const string Boolean = "bool";
        public const string Settings = "settings";
    }

    public enum Events
    {
        // for ScreenSwitcher
        AddPatient,
        OpenPatient,
        OpenCourse,
        EditPatient,
        ShowPatient,
        LeavePatientEditor,
        OpenHealthRecord,
        EditHealthRecord,

        OpenSettings,
        SettingsSaved,

        // for Search

        SendToSearch,
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

        public static KeyValuePair<string, object>[] AsParams(this object[] objs, string key)
        {
            return new[] { new KeyValuePair<string, object>(key, objs) };
        }
    }
}