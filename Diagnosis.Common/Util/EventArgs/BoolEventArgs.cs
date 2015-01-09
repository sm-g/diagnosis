using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    [Serializable]
    public class BoolEventArgs : EventArgs
    {
        public readonly bool value;

        [System.Diagnostics.DebuggerStepThrough]
        public BoolEventArgs(bool value)
        {
            this.value = value;
        }
    }
}
