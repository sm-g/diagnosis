using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Controls
{
    public class DatePickerViewModel : ViewModelBase
    {
        private DateOffset d;

        public DatePickerViewModel(DateOffset d)
        {
            this.d = d;
            d.PropertyChanged += d_PropertyChanged;
        }

        public int? Year
        {
            get { return d.Year; }
            set { d.Year = value; }
        }

        public int? Month
        {
            get { return d.Month; }
            set { d.Month = value; }
        }

        public int? Day
        {
            get { return d.Day; }
            set { d.Day = value; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    d.PropertyChanged -= d_PropertyChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void d_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Day":
                case "Month":
                case "Year":
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }
    }
}
