using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CourseEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CourseEditorViewModel));
        private readonly Course course;

        private CourseViewModel _courseVm;

        public CourseEditorViewModel(Course course)
        {
            this.course = course;
            Course = new CourseViewModel(course);

            course.PropertyChanged += course_PropertyChanged;
            (course as IEditableObject).BeginEdit();
            foreach (var app in course.Appointments)
                (app as IEditableObject).BeginEdit();

            Title = "Редактирование курса";
            HelpTopic = "editholder";
            WithHelpButton = true;
        }
        public CourseViewModel Course
        {
            get
            {
                return _courseVm;
            }
            set
            {
                if (_courseVm != value)
                {
                    _courseVm = value;
                    OnPropertyChanged(() => Course);
                }
            }
        }

        public RelayCommand ToggleIsEndedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (course.IsEnded)
                        course.Open();
                    else
                        course.Finish();
                });
            }
        }

        public RelayCommand CorrectAppsDatesCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Contract.Requires(course.Appointments.All(x => x.InEdit));
                    foreach (var app in course.Appointments)
                        app.FitDateToCourse();

                    OnPropertyChanged(() => Course);
                    OnPropertyChanged(() => CanCorrectAppsDates);
                });
            }
        }

        public bool CanCorrectAppsDates
        {
            get
            {
                return !course.IsValid() && (!course.IsEnded || course.Start <= course.End) // ошибка не в самом курсе (начало>конец)
                    && course.GetErrors().Any(x => x.PropertyName == "Start" ||
                                                   x.PropertyName == "End");
            }
        }

        public override bool CanOk
        {
            get
            {
                return course.IsValid();
            }
        }

        protected override void OnOk()
        {
            foreach (var app in course.Appointments)
                (app as IEditableObject).EndEdit();
            (course as IEditableObject).EndEdit();

            Session.DoDelete(course); // cascade apps
        }

        protected override void OnCancel()
        {
            foreach (var app in course.Appointments)
                (app as IEditableObject).CancelEdit();
            (course as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                course.PropertyChanged -= course_PropertyChanged;
                Course.Dispose();
            }
            base.Dispose(disposing);
        }

        private void course_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(() => CanCorrectAppsDates);
        }
    }
}