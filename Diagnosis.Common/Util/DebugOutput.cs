using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Diagnosis.Common
{
    public class DebugOutput
    {
        public DebugOutput(int printMemoryUsageIn)
        {
            if (printMemoryUsageIn > 0)
            {
                var debugThread = new Thread(PrintMemoryUsage) { IsBackground = true };
                debugThread.Start(printMemoryUsageIn);
            }
        }

        public static int GetSubscriberCount(EventHandler eventHandler)
        {
            var count = 0;
            if (eventHandler != null)
            {
                count = eventHandler.GetInvocationList().Length;
            }
            Debug.Print("{0} has {1} subscribers", eventHandler, count);
            return count;
        }

        public void PrintMemoryUsage(object pause)
        {
            while (true)
            {
                Debug.Print("total memory = {0}", GC.GetTotalMemory(true));
                Thread.Sleep((int)pause);
            }
        }
    }
}
