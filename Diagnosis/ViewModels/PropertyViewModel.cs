using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PropertyViewModel : ViewModelBase
    {
        private Property property;
        private PropertyValue _selectedValue;

        public string Title
        {
            get
            {
                return property.Title;
            }
            set
            {
                if (property.Title != value)
                {
                    property.Title = value;
                    OnPropertyChanged(() => Title);
                }
            }
        }

        public int PropertyId
        {
            get
            {
                return property.Id;
            }
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
            property = p;

            Values = new ObservableCollection<PropertyValue>(p.Values);
        }
    }
}