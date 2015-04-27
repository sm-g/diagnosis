using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Diagnosis.Common.Types
{
    [DebuggerDisplay("{ConnectionString} {ProviderName}")]
    public struct ConnectionInfo
    {
        public ConnectionInfo(string constr, string providerName)
            : this()
        {
            if (constr != null && !constr.StartsWith("Data Source="))
                constr = "Data Source=" + constr;
            ConnectionString = constr;
            ProviderName = providerName;
        }
        public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }

        public static bool operator ==(ConnectionInfo c1, ConnectionInfo c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(ConnectionInfo c1, ConnectionInfo c2)
        {
            return !c1.Equals(c2);
        }
    }
}
