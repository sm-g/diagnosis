using Diagnosis.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class CoursesManager : ViewModelBase
    {
        private CourseViewModel _selectedCourse;
        PatientViewModel patientVM;

        public ObservableCollection<CourseViewModel> Courses
        {
            get;
            private set;
        }

        public CourseViewModel SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;

                    OnPropertyChanged(() => SelectedCourse);
                }
            }
        }

        public void AddCourse(Course course)
        {
            Contract.Requires(course != null);

            var courseVM = new CourseViewModel(course);
            courseVM.Editable.Deleted += course_Deleted;
            Courses.Add(courseVM);
        }

        public CoursesManager(PatientViewModel patientVM)
        {
            Contract.Requires(patientVM != null);

            this.patientVM = patientVM;

            Courses = new ObservableCollection<CourseViewModel>(patientVM.patient.Courses.Select(i => new CourseViewModel(i)));
            foreach (var course in Courses)
            {
                course.Editable.Deleted += course_Deleted;
            }

            if (Courses.Count > 0)
            {
                SelectedCourse = Courses.Last();
            }
        }

        private void course_Deleted(object sender, EventArgs e)
        {
            var courseVM = sender as CourseViewModel;
            patientVM.patient.DeleteCourse(courseVM.course);
            patientVM.Editable.MarkDirty();
            Courses.Remove(courseVM);
        }
    }
}