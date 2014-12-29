using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public interface IClipboardTarget
    {
        void Cut();
        void Copy();
        void Paste();
    }
}
