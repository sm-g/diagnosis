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

        private IList<ConfindenceHrItemObject> itemobjects;

        public IList<ConfindenceHrItemObject> ItemObjects { get { return itemobjects; } }

        public TagData(IList<ConfindenceHrItemObject> itemobjects)
        {
            this.itemobjects = itemobjects;
        }
    }
}
