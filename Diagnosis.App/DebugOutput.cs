using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.ViewModels;
using Diagnosis.Models;
using System.Diagnostics;
using System.Threading;

namespace Diagnosis.App
{
    public class DebugOutput
    {
        bool showPropertySelectedValueChanged = false;

        public DebugOutput()
        {
            var debugThread = new Thread(PrintMemoryUsage) { IsBackground = true };
            //  debugThread.Start();           
        }

        public static int GetSubscriberCount(EditableEventHandler eventHandler)
        {
            var count = 0;
            if (eventHandler != null)
            {
                count = eventHandler.GetInvocationList().Length;
            }
            Debug.Print("{0} has {1} subscribers", eventHandler, count);
            return count;
        }

        public void PrintMemoryUsage()
        {
            while (true)
            {
                Debug.Print("total memory {0}", GC.GetTotalMemory(true));
                Thread.Sleep(5000);
            }
        }
    }
}
