using Diagnosis.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class CoursesManager : ViewModelBase
    {
        private CourseViewModel _selectedCourse;
        private PatientViewModel patientVM;

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
            var courseVM = new CourseViewModel(course);
            SubscribeCourse(courseVM);
            courseVM.Editable.CanBeDeleted = true;

            Courses.Add(courseVM);
            Courses = new ObservableCollection<CourseViewModel>(
                Courses.OrderByDescending(cvm => cvm.course, new CompareCourseByDate()));

            SelectedCourse = courseVM;

            OnPropertyChanged(() => Courses);
        }

        public void OpenHr(HealthRecord hr)
        {
            var course = Courses.Where(x => x.course == hr.Appointment.Course).First();
            SelectedCourse = course;
            var app = course.Appointments.Where(x => x.appointment == hr.Appointment).First();
            course.SelectedAppointment = app;
            var hrVM = app.HealthRecords.Where(x => x.healthRecord == hr).First();
            app.SelectedHealthRecord = hrVM;
        }

        public void UnsubscribeSelectedHr()
        {
            if (SelectedCourse != null &&
                SelectedCourse.SelectedAppointment != null &&
                SelectedCourse.SelectedAppointment.SelectedHealthRecord != null)
            {
                SelectedCourse.SelectedAppointment.SelectedHealthRecord.UnsubscribeCheckedChanges();
            }
        }

        public CoursesManager(PatientViewModel patientVM)
        {
            Contract.Requires(patientVM != null);

            this.patientVM = patientVM;

            var courseVMs = patientVM.patient.Courses.
                Select(i => new CourseViewModel(i)).
                OrderByDescending(cvm => cvm.course, new CompareCourseByDate());

            Courses = new ObservableCollection<CourseViewModel>(courseVMs);

            foreach (var course in Courses)
            {
                SubscribeCourse(course);
            }

            if (Courses.Count > 0)
            {
                SelectedCourse = Courses[0];
            }
        }

        private void SubscribeCourse(CourseViewModel course)
        {
            course.Editable.Deleted += course_Deleted;
        }

        private void course_Deleted(object sender, EditableEventArgs e)
        {
            var courseVM = e.viewModel as CourseViewModel;
            courseVM.Editable.Deleted -= course_Deleted;

            patientVM.patient.DeleteCourse(courseVM.course);
            patientVM.Editable.MarkDirty();
            Courses.Remove(courseVM);
        }
    }
}