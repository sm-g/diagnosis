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
        private readonly Patient patient;
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
                    IList<CourseViewModel> courseVMs;
                    using (var tester = new PerformanceTester((ts) => Debug.Print("making courses for {0}: {1}", patient, ts)))
                    {
                        courseVMs = patient.Courses
                           .OrderByDescending(c => c, new CompareCourseByDate())
                           .Select(i => new CourseViewModel(i))
                           .ToList();
                    }
                    _courses = new ObservableCollection<CourseViewModel>(courseVMs);
                    OnCoursesLoaded();
                }
                return _courses;
            }
        }

        public CoursesManager(Patient patient)
        {
            this.patient = patient;
            patient.Courses.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (Course item in e.NewItems)
                    {
                        var courseVM = new CourseViewModel(item);
                        Courses.Insert(0, courseVM);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (Course item in e.OldItems)
                    {
                        var courseVM = Courses.Where(vm => vm.course == item).FirstOrDefault();
                        Courses.Remove(courseVM);
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

    }
}