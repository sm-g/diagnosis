using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.ViewModels.DataTransfer
{
    [Serializable]
    public class TagData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("diagnosis.tag");

        private IList<ConfWithHio> itemobjects;

        public IList<ConfWithHio> ItemObjects { get { return itemobjects; } }

        public TagData(IList<ConfWithHio> itemobjects)
        {
            this.itemobjects = itemobjects;
        }
    }
}
