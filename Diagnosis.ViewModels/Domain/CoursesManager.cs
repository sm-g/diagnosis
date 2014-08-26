using Diagnosis.Core;
using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class CoursesManager
    {
        private readonly PatientViewModel patientVM;
        private ObservableCollection<CourseViewModel> _courses;

        public event EventHandler CoursesLoaded;

        /// <summary>
        /// Курсы пацента, отсортированы по дате по убыванию (нулевой — самый поздний курс).
        /// </summary>
        public ObservableCollection<CourseViewModel> Courses
        {
            get
            {
                if (_courses == null)
                {
                    _courses = MakeCourses();
                    patientVM.SubscribeEditableNesting(Courses);
                    OnCoursesLoaded();
                }
                return _courses;
            }
        }

        private CourseViewModel AddCourse(Course course)
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
            this.patientVM = patientVM;
            patientVM.patient.Courses.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (Course item in e.NewItems)
                    {
                        AddCourse(item);
                    }
                }
            };
        }

        protected virtual void OnCoursesLoaded()
        {
            var h = CoursesLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<CourseViewModel> MakeCourses()
        {
            IList<CourseViewModel> courseVMs;
            using (var tester = new PerformanceTester((ts) => Debug.Print("making courses for {0}: {1}", patientVM, ts)))
            {
                courseVMs = patientVM.patient.Courses
                   .OrderByDescending(c => c, new CompareCourseByDate())
                   .Select(i => new CourseViewModel(i))
                   .ToList();
            }

            courseVMs.ForAll(x => SubscribeCourse(x));
            return new ObservableCollection<CourseViewModel>(courseVMs);
        }

        #region Course stuff

        private void SubscribeCourse(CourseViewModel courseVM)
        {
            courseVM.Editable.Deleted += course_Deleted;
            courseVM.Editable.Committed += course_Committed;
        }

        private void UnsubscribeCourse(CourseViewModel courseVM)
        {
            courseVM.Editable.Deleted -= course_Deleted;
            courseVM.Editable.Committed -= course_Committed;
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
            patientVM.patient.Courses.Remove(course);

            var courseVM = Courses.Where(vm => vm.course == course).FirstOrDefault();
            Courses.Remove(courseVM);

            UnsubscribeCourse(courseVM);
        }
            
        #endregion Course stuff
    }
}