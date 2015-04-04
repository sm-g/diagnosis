using Diagnosis.ViewModels.Controls;
using System;
using System.Linq;

namespace Diagnosis.Common.Presentation.DesignData
{
    public class SampleFilterViewModel : FilterViewModel<string>
    {
        public SampleFilterViewModel()
            : base(null)
        {
        }
    }
}