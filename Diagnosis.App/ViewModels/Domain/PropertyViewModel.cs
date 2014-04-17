﻿using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    public class PropertyViewModel : EditableBase
    {
        internal readonly Property property;
        private PropertyValue _selectedValue;

        public string Name
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
                    OnPropertyChanged(() => Name);
                }
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
            property = p;

            Values = new ObservableCollection<PropertyValue>(p.Values);
        }
    }
}