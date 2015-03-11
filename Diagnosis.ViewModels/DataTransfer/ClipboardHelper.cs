using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Diagnosis.ViewModels
{
    public static class ClipboardHelper
    {
        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        public static void LogHrItemObjects(this log4net.ILog logger, string action, IEnumerable<IHrItemObject> hios)
        {
            logger.DebugFormat("{0} hios: {1}", action, hios.FlattenString());
        }
        public static void LogHrItemObjects(this log4net.ILog logger, string action, IEnumerable<ConfindenceHrItemObject> chios)
        {
            logger.DebugFormat("{0} hios: {1}", action, chios.FlattenString());
        }
        public static void LogHrs(this log4net.ILog logger, string action, IEnumerable<HrData.HrInfo> hrs)
        {
            logger.DebugFormat("{0} hrs with hios: {1}", action, string.Join("\n", hrs.Select((hr, i) => string.Format("{0} {1}", i, hr.Hios.FlattenString()))));
        }
    }
}