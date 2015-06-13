using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Controls.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Client.App.DesignData
{
    #region Screens

    public class SampleSearchViewModel : SearchViewModel
    {
        public SampleSearchViewModel()
        {
            ControlsVisible = true;
        }
    }

    public class SampleCardViewModel : CardViewModel
    {
        public SampleCardViewModel()
        {
        }
    }

    public class SampleHrListViewModel : HrListViewModel
    {
        public SampleHrListViewModel() :
            base(Mocks.course, SessionVMBase.Nhib.GetSession())
        {
        }
    }

    public class SampleIcdSelectorViewModel : IcdSelectorViewModel
    {
        public SampleIcdSelectorViewModel()
            : base("перикардит")
        {
        }
    }

    public class SampleAdminSettingsViewModel : AdminSettingsViewModel
    {
        public SampleAdminSettingsViewModel()
            : base(Mocks.admin)
        {
        }
    }

    public class SampleVocabularyListViewModel : VocabularySyncViewModel
    {
        public SampleVocabularyListViewModel()
        {
            Vocs.Add(new VocabularyViewModel(Mocks.voc));
            AvailableVocs.Add(new VocabularyViewModel(Mocks.voc2));
        }
    }

    #endregion Screens

    #region Editors

    public class SampleCriterionEditorViewModel : CriterionEditorViewModel
    {
        public SampleCriterionEditorViewModel()
            : base(Mocks.crit) { }
    }

    public class SampleCriteriaGroupEditorViewModel : CriteriaGroupEditorViewModel
    {
        public SampleCriteriaGroupEditorViewModel()
            : base(Mocks.crg) { }
    }

    public class SampleEstimatorEditorViewModel : EstimatorEditorViewModel
    {
        public SampleEstimatorEditorViewModel()
            : base(Mocks.est) { }
    }

    public class SampleDoctorEditorViewModel : DoctorEditorViewModel
    {
        public SampleDoctorEditorViewModel()
            : base(Mocks.doc)
        {
        }
    }

    public class SampleHrEditorViewModel : HrEditorViewModel
    {
        public SampleHrEditorViewModel()
            : base(SessionVMBase.Nhib.GetSession())
        {
        }

        public new IEnumerable<HrCategory> Categories
        {
            get
            {
                return Mocks.cats;
            }
        }

        public new HrCategory Category
        {
            get { return Mocks.cats[0]; }
            set { }
        }

        public new HealthRecordViewModel HealthRecord
        {
            get
            {
                return new HealthRecordViewModel(Mocks.hr)
                {
                };
            }
        }
    }

    public class SampleCourseEditorViewModel : CourseEditorViewModel
    {
        public SampleCourseEditorViewModel()
            : base(Mocks.course2)
        {
        }
    }

    public class SampleAppointmentEditorViewModel : AppointmentEditorViewModel
    {
        public SampleAppointmentEditorViewModel()
            : base(Mocks.app)
        {
        }
    }

    public class SampleMeasureEditorViewModel : MeasureEditorViewModel
    {
        public SampleMeasureEditorViewModel()
            : base(new Measure(36, Mocks.uom) { Word = Mocks.word }, true)
        {
        }
    }

    public class SampleDateEditorViewModel : DateEditorViewModel
    {
        public SampleDateEditorViewModel()
            : base(Mocks.hr)
        {
            IsDateEditorExpanded = true;
        }
    }

    public class SampleQueryEditorViewModel : QueryEditorViewModel
    {
        public SampleQueryEditorViewModel()
            : base(SessionVMBase.Nhib.GetSession())
        {
            QueryBlocks.Add(new SampleQueryBlockViewModel());
        }
    }

    #endregion Editors

    #region Parts

    public class SamplePatResultViewModel : CritResultItemViewModel
    {
        public SamplePatResultViewModel()
            : base(Mocks.pat, new[] { Mocks.hr }, new[] { Mocks.crit }) { }
    }

    public class SampleQueryBlockViewModel : QueryBlockViewModel
    {
        public SampleQueryBlockViewModel()
            : base(SessionVMBase.Nhib.GetSession(), () => { })
        {
            if (IsRoot)
                Children.Add(new SampleQueryBlockViewModel());
        }
    }

    public class SampleTagViewModel : TagViewModel
    {
        public SampleTagViewModel()
            : base(new Mock<ITagParentAutocomplete>().Object)
        {
            Blank = Mocks.word;
            IsDraggable = true;
        }
    }

    public class SampleHealthRecordViewModel : HealthRecordViewModel
    {
        public SampleHealthRecordViewModel()
            : base(Mocks.hr)
        {
        }
    }

    public class SampleEventDateViewModel : EventDateViewModel
    {
        public SampleEventDateViewModel()
            : base(Mocks.hr)
        {
        }
    }

    public class SampleHeaderViewModel : HeaderViewModel
    {
        public SampleHeaderViewModel()
            : base(Mocks.course)
        {
        }
    }

    public class SampleShortHealthRecordViewModel : ShortHealthRecordViewModel
    {
        public SampleShortHealthRecordViewModel()
            : base(Mocks.hr)
        {
            SortingExtraInfo = Mocks.hr.Category.Title;
        }
    }

    #endregion Parts

    public class Mocks
    {
        public static Patient pat = new Patient("Иванов", "Иван") { BirthYear = 2000 };
        public static Doctor doc = new Doctor("Ivanov", "Ivan");
        public static Admin admin = new Admin(new Passport(doc));

        public static Course course = new Course(pat, doc)
        {
            Start = DateTime.Now
        };

        public static Course course2 = new Course(pat, doc)
        {
            Start = DateTime.Now.AddDays(-4),
            End = DateTime.Now
        };

        public static Appointment app = new Appointment(course, doc) { DateAndTime = DateTime.Now.AddDays(-4) };

        public static Word word = new Word("анемия");
        public static Word word2 = new Word("впервые");
        public static UomType uomType = new UomType("температура", 1);
        public static Uom uom = new Uom("C", 36, uomType);
        public static Vocabulary voc = new Vocabulary("словарь");
        public static Vocabulary voc2 = new Vocabulary("словарь 2");

        public static Estimator est = new Estimator() { Description = "est with very long description" };
        public static CriteriaGroup crg = new CriteriaGroup(est) { Description = "crgr with very long description" };
        public static Criterion crit = new Criterion(crg) { Description = "crit with very long description", Code = "1", Value = "10", Options = "opts" };

        public static HrCategory[] cats = new[] {
            new HrCategory("Жалоба",1),
            new HrCategory("История",2),
            HrCategory.Null
        };

        public static HealthRecord hr;

        static Mocks()
        {
            hr = new HealthRecord(course, doc)
            {
                Category = cats[0],
            };
            hr.FromDate.Month = 5;
            hr.AddItems(new IHrItemObject[] { word, word2, new Comment("без осложнений") });
        }
    }
}