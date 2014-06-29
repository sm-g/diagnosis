using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace Diagnosis.App.Windows
{
    public class MyTraceListener : TraceListener
    {
        private TextBoxBase output;

        public MyTraceListener(TextBoxBase output)
        {
            this.Name = "Trace";
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
                output.Dispatcher.Invoke(append);
            }
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
