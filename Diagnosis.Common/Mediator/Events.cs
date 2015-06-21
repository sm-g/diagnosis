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
        public readonly static string Entity = "entity";
        public readonly static string Word = "word";
        public readonly static string Patient = "patient";
        public readonly static string Doctor = "doctor";
        public readonly static string User = "user";
        public readonly static string Course = "course";
        public readonly static string Appointment = "appointment";
        public readonly static string HealthRecord = "healthRecord";
        public readonly static string HealthRecords = "healthRecords";
        public readonly static string Category = "category";
        public readonly static string Holder = "holder";
        public readonly static string Uom = "uom";
        public readonly static string Crit = "crit";

        // other
        public readonly static string ToSearchPackage = "package";
        public readonly static string UndoDoActions = "undooverlay";
        public readonly static string Type = "type";

        public readonly static string Dialog = "dialog";
        public readonly static string Window = "window";

        public readonly static string String = "string";
        public readonly static string Boolean = "bool";

        public readonly static string Name = "name";
        public readonly static string Value = "value";
        public readonly static string Session = "session";
    }

    public enum Event
    {
        // for ScreenSwitcher
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
        DeleteHolder,
        AddHr,

        // 
        DeleteCrit,

        // nhibernate
        EntitySaved,
        EntityPersisted,
        EntityDeleted,

        // other
        ChangeTheme,
        ChangeFont,

        ShowHelp,
        ShowUndoOverlay,
        ShowMessageOverlay,
        HideOverlay,

        // app
        PushToSettings,
        Shutdown,
        NewSession,
        SaveLayout,
    }


}