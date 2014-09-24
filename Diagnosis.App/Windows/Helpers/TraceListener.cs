using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using log4net;

namespace Diagnosis.App.Windows
{
    public class TextBoxTraceListener : TraceListener
    {
        private TextBoxBase output;

        public TextBoxTraceListener(TextBoxBase output)
        {
            this.Name = "DebugWindowListener";
            this.output = output;
        }

        public override void Write(string message)
        {

            Action append = delegate()
            {
                output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString("HH:mm:ss")));
                output.AppendText(message);
            };

            if (output.Dispatcher.CheckAccess())
            {
                append();
            }
            else
            {
                output.Dispatcher.BeginInvoke(append);
            }
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
