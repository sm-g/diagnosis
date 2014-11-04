using Diagnosis.Models;
using log4net;
using System.ComponentModel;

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
            (course as IEditableObject).BeginEdit();

            Title = "Редактирование курса";
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

        public override bool CanOk
        {
            get
            {
                return course.IsValid();
            }
        }

        protected override void OnOk()
        {
            (course as IEditableObject).EndEdit();

            using (var t = Session.BeginTransaction())
            {
                try
                {
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }
            }
        }

        protected override void OnCancel()
        {
            (course as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Course.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}