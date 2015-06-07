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
        // domain
        public static string Word = "word";
        public static string Patient = "patient";
        public static string Doctor = "doctor";
        public static string User = "user";
        public static string Course = "course";
        public static string Appointment = "appointment";
        public static string HealthRecord = "healthRecord";
        public static string HealthRecords = "healthRecords";
        public static string Category = "category";
        public static string Holder = "holder";
        public static string Uom = "uom";
        public static string Crit = "crit";

        // other
        public static string ToSearchPackage = "package";
        public static string UndoDoActions = "undooverlay";
        public static string Type = "type";

        public static string Dialog = "dialog";
        public static string Window = "window";

        public static string String = "string";
        public static string Boolean = "bool";

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
        OpenCrit,

        // windows
        EditDoctor,
        EditPatient,
        EditHolder,
        EditCrit,
        EditWord,
        EditUom,
        OpenSettings,
        OpenDialog,
        OpenWindow,

        // card logic
        SendToSearch,
        ShowUndoOverlay,
        ShowMessageOverlay,
        HideOverlay,
        DeleteHolder,
        AddHr,

        // 
        DeleteCrit,

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
        SaveLayout,
    }


}