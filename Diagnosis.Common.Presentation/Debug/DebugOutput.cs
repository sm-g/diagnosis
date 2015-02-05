using System;
using System.Linq;
using System.Threading;

namespace Diagnosis.Common.Presentation.DebugTools
{
    public class DebugOutput
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(DebugOutput));

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
            logger.DebugFormat("{0} has {1} subscribers", eventHandler, count);
            return count;
        }

        public void PrintMemoryUsage(object pause)
        {
            while (true)
            {
                logger.DebugFormat("total memory = {0}", GC.GetTotalMemory(true));
                Thread.Sleep((int)pause);
            }
        }
    }
}