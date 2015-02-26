using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.Models;
using System.Diagnostics;
using Diagnosis.ViewModels.Screens;

namespace Diagnosis.App.Controls
{
    public class HrsHolderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PatientTemplate { get; set; }
        public DataTemplate CourseTemplate { get; set; }
        public DataTemplate AppointmentTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if (item is IHrsHolder)
            {
                return Switch(item as IHrsHolder);
            }

            if (item is PatientViewModel)
            {
                return PatientTemplate;
            }
            if (item is CourseViewModel)
            {
                return CourseTemplate;
            }
            if (item is AppointmentViewModel)
            {
                return AppointmentTemplate;
            }

            dynamic vm = item; // vm with Holder prop

            return Switch(vm.Holder);
        }

        // cannot use GetType because Nhibernate proxy
        DataTemplate Switch(IHrsHolder holder)
        {
            if (holder is Patient)
            {
                return PatientTemplate;
            }
            if (holder is Course)
            {
                return CourseTemplate;
            }
            if (holder is Appointment)
            {
                return AppointmentTemplate;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

}
