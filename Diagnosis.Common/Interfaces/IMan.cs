using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Core
{
    public interface IMan
    {
        string LastName { get; }
        string FirstName { get; }
        string MiddleName { get; }
    }
}
