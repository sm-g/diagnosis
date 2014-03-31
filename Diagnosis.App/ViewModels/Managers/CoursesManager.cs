﻿using Diagnosis.Models;
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
            var courseVM = new CourseViewModel(course);
            courseVM.Editable.Deleted += course_Deleted;

            Courses.Add(courseVM);
            Courses = new ObservableCollection<CourseViewModel>(
                Courses.OrderByDescending(cvm => cvm.course, new CompareCourseByDate()));

            SelectedCourse = Courses[0];

            OnPropertyChanged(() => Courses);
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
                course.Editable.Deleted += course_Deleted;
            }

            if (Courses.Count > 0)
            {
                SelectedCourse = Courses[0];
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