using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Diagnosis.ViewModels;
using Diagnosis.Models;

namespace Diagnosis.App.Controls.Screens
{
    public class CurrentHrsHolderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PatientTemplate { get; set; }
        public DataTemplate CourseTemplate { get; set; }
        public DataTemplate AppointmentTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PatientViewModel)
            {
                return PatientTemplate;
            }
            if (item is CourseViewModel1)
            {
                return CourseTemplate;
            }
            if (item is AppointmentViewModel)
            {
                return AppointmentTemplate;
            }

            return null;
        }
    }

}
