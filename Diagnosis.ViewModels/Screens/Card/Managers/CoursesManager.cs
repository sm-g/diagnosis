using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CoursesManager : DisposableBase
    {
        private readonly Patient patient;
        private ObservableCollection<ShortCourseViewModel> _courses;

        public event EventHandler CoursesLoaded;
        private NotifyCollectionChangedEventHandler onCoursesChanged;

        /// <summary>
        /// Курсы пацента, отсортированы по дате по убыванию (нулевой — самый поздний курс).
        /// </summary>
        public ObservableCollection<ShortCourseViewModel> Courses
        {
            get
            {
                if (_courses == null)
                {
                    IList<ShortCourseViewModel> courseVMs;
                    courseVMs = patient.Courses
                       .OrderByDescending(c => c, new CompareCourseByDate())
                       .Select(i => new ShortCourseViewModel(i))
                       .ToList();

                    _courses = new ObservableCollection<ShortCourseViewModel>(courseVMs);
                    OnCoursesLoaded();
                }
                return _courses;
            }
        }

        public CoursesManager(Patient patient, NotifyCollectionChangedEventHandler onCoursesChanged)
        {
            this.patient = patient;
            this.onCoursesChanged = onCoursesChanged;

            patient.CoursesChanged += patient_CoursesChanged;
            patient.CoursesChanged += onCoursesChanged;
        }

        private void patient_CoursesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Course item in e.NewItems)
                {
                    var courseVM = new ShortCourseViewModel(item);
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
        }

        protected virtual void OnCoursesLoaded()
        {
            var h = CoursesLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    patient.CoursesChanged -= patient_CoursesChanged;
                    patient.CoursesChanged -= onCoursesChanged;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}