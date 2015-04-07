using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Server.App.DesignData
{
    public class SampleUomEditorViewModel : UomEditorViewModel
    {
        public SampleUomEditorViewModel()
            : base(new Uom("kg", 1, new UomType("mass", 1)))
        {
        }
    }
    public class SampleVocabularyEditorViewModel : VocabularyEditorViewModel
    {
        public SampleVocabularyEditorViewModel()
            : base(new Vocabulary("1"))
        {
        }
    }
}
