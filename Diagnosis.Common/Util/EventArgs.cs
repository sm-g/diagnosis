using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Common
{
    [Serializable]
    public class ObjectEventArgs : EventArgs
    {
        public readonly object arg;

        [System.Diagnostics.DebuggerStepThrough]
        public ObjectEventArgs(object arg)
        {
            this.arg = arg;
        }
    }

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

    [Serializable]
    public class ListEventArgs<T> : EventArgs
    {
        public readonly IList<T> list;

        [System.Diagnostics.DebuggerStepThrough]
        public ListEventArgs(IList<T> list)
        {
            this.list = list;
        }
    }

    [Serializable]
    public class StringEventArgs : EventArgs
    {
        public readonly string str;

        [System.Diagnostics.DebuggerStepThrough]
        public StringEventArgs(string arg)
        {
            this.str = arg;
        }
    }
}