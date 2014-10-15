using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.ViewModels;
using Diagnosis.Models;
using System.Diagnostics;

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

            var @switch = new Dictionary<Type, Func<DataTemplate>> {
                { typeof(Patient), () => PatientTemplate },
                { typeof(Course), () => CourseTemplate },
                { typeof(Appointment), () => AppointmentTemplate }
           };

            if (item is IHrsHolder)
            {
                return @switch[item.GetType()]();
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

            return @switch[vm.Holder.GetType()]();
        }
    }

}
