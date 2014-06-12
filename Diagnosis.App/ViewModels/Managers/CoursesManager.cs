﻿using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class CoursesManager
    {
        private readonly PatientViewModel patientVM;

        /// <summary>
        /// Курсы пацента, отсортированы по дате по убыванию (нулевой — самый поздний курс).
        /// </summary>
        public ObservableCollection<CourseViewModel> Courses
        {
            get;
            private set;
        }

        public CourseViewModel AddCourse(Course course)
        {
            var courseVM = new CourseViewModel(course);
            SubscribeCourse(courseVM);

            Courses.Add(courseVM);
            if (Courses.Count > 1)
                Courses.Move(Courses.Count - 1, 0);

            return courseVM;
        }

        public CoursesManager(PatientViewModel patientVM)
        {
            Contract.Requires(patientVM != null);

            this.patientVM = patientVM;

            SetupCourses();
        }

        private void SetupCourses()
        {
            var courseVMs = patientVM.patient.Courses
                .Select(i => new CourseViewModel(i))
                .OrderByDescending(cvm => cvm.course, new CompareCourseByDate())
                .ToList();

            courseVMs.ForAll(x => SubscribeCourse(x));
            Courses = new ObservableCollection<CourseViewModel>(courseVMs);
        }

        #region Course stuff

        private void SubscribeCourse(CourseViewModel courseVM)
        {
            courseVM.Editable.Deleted += course_Deleted;
            courseVM.Editable.Committed += course_Committed;
            courseVM.Editable.DirtyChanged += course_DirtyChanged;
        }

        private void course_Committed(object sender, EditableEventArgs e)
        {
            var course = e.entity as Course;
            ISession session = NHibernateHelper.GetSession();
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(course);
                transaction.Commit();
            }
        }

        private void course_Deleted(object sender, EditableEventArgs e)
        {
            var course = e.entity as Course;
            patientVM.patient.DeleteCourse(course);

            var courseVM = Courses.Where(vm => vm.course == course).FirstOrDefault();
            courseVM.Editable.Deleted -= course_Deleted;
            courseVM.Editable.Committed -= course_Committed;
            courseVM.Editable.DirtyChanged -= course_DirtyChanged;

            Courses.Remove(courseVM);
        }

        private void course_DirtyChanged(object sender, EditableEventArgs e)
        {
            patientVM.Editable.IsDirty = Courses.Any(x => x.Editable.IsDirty);
        }

        #endregion Course stuff
    }
}