using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Sync
{
    public static class Poster
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Poster));

        public static event EventHandler<StringEventArgs> MessagePosted;

        public static void PostMessage(string str)
        {
            logger.DebugFormat(str);
            Send(str);
        }

        public static void PostMessage(string str, params object[] p)
        {
            logger.DebugFormat(string.Format(str, p));
            Send(string.Format(str, p));
        }

        public static void PostMessage(Exception ex)
        {
            logger.WarnFormat(ex.ToString());
            Send(ex.Message);
        }

        private static void Send(string str)
        {
            var h = MessagePosted;
            if (h != null)
            {
                h(typeof(Syncer), new StringEventArgs(str));
            }
        }
    }
}
