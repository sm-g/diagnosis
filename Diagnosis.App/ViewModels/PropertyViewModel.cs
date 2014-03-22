using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Diagnosis.App;

namespace Diagnosis.App.ViewModels
{
    public class PropertyViewModel : EditableBase
    {
        private PropertyValue _selectedValue;

        #region EditableBase

        public override string Name
        {
            get
            {
                return Property.Title;
            }
            set
            {
                if (Property.Title != value)
                {
                    Property.Title = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        #endregion EditableBase

        internal Property Property
        {
            get;
            private set;
        }

        public PropertyValue SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    OnPropertyChanged(() => SelectedValue);

                    this.Send((int)EventID.PropertySelectedValueChanged, new PropertySelectedValueChangedParams(this).Params);
                }
            }
        }

        public ObservableCollection<PropertyValue> Values
        {
            get;
            private set;
        }

        public PropertyViewModel(Property p)
        {
            Contract.Requires(p != null);
            Property = p;

            Values = new ObservableCollection<PropertyValue>(p.Values);
        }
    }
}