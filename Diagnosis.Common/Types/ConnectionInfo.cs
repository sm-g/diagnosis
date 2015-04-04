using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common.Types
{
    public class ConnectionInfo
    {
        public ConnectionInfo(string constr, string providerName)
        {
            if (!constr.StartsWith("Data Source="))
                constr = "Data Source=" + constr;
            ConnectionString = constr;
            ProviderName = providerName;
        }
        public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }
    }
}
