using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Diagnosis.ViewModels.DataTransfer
{
    [Serializable]
    public class HrData
    {
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("diagnosis.hr");
        private IList<HrInfo> hrs;

        public IList<HrInfo> Hrs { get { return hrs; } }

        public HrData(IList<HrInfo> hrs)
        {
            this.hrs = hrs;
        }

        [Serializable]
        public class HrInfo
        {
            public Guid HolderId { get; set; }

            public Guid DoctorId { get; set; }

            public Guid? CategoryId { get; set; }

            public DateOffset From { get; set; }

            public DateOffset To { get; set; }

            public HealthRecordUnit Unit { get; set; }

            public List<ConfindenceHrItemObject> Chios { get; set; }
        }
    }
}
