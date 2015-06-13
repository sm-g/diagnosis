using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Controls.Autocomplete
{
    public class SuggestionViewModel : ViewModelBase
    {
        private IHrItemObject _hio;

        private bool _absent;

        private bool _new;

        public SuggestionViewModel(IHrItemObject hio, bool isAlt, bool isNew)
        {
            Contract.Requires(hio != null);
            Hio = hio;
            IsAlter = isAlt;
            IsNew = isNew;
        }
        public IHrItemObject Hio
        {
            get
            {
                return _hio;
            }
            set
            {
                if (_hio != value)
                {
                    _hio = value;
                    OnPropertyChanged(() => Hio);
                }
            }
        }
        public bool IsAlter
        {
            get
            {
                return _absent;
            }
            set
            {
                if (_absent != value)
                {
                    _absent = value;
                    OnPropertyChanged(() => IsAlter);
                }
            }
        }
        public bool IsNew
        {
            get
            {
                return _new;
            }
            set
            {
                if (_new != value)
                {
                    _new = value;
                    OnPropertyChanged(() => IsNew);
                }
            }
        }

        public override string ToString()
        {
            return Hio.ToString();
        }
    }
}