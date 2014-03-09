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

        public ObservableCollection<PropertyValue> Values
        {
            get;
            private set;
        }

        public PropertyViewModel AddValue(string value)
        {
            Values.Add(new PropertyValue(value, property));
            return this;
        }

        public PropertyViewModel(Property p)
        {
            Contract.Requires(p != null);
            property = p;

            Values = new ObservableCollection<PropertyValue>();
        }

        public PropertyViewModel(string name)
        {
            Contract.Requires(name != null);
            Contract.Requires(name != "");
            property = new Property(name);

            Values = new ObservableCollection<PropertyValue>();
        }
    }
}