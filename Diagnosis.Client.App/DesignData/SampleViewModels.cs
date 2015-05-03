using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Client.App.DesignData
{
    public class SampleSearchViewModel : SearchViewModel
    {
        public SampleSearchViewModel()
        {
            ControlsVisible = true;
        }
    }


    public class SampleQueryBlockViewModel : QueryBlockViewModel
    {
#pragma warning disable 0618
        public SampleQueryBlockViewModel()
        {
            Children.Add(new SampleQueryBlockViewModel());
        }
#pragma warning restore 0618
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

#pragma warning disable 0618

    // ctor for xaml
    public class SampleHrEditorViewModel : HrEditorViewModel
#pragma warning restore 0618
    {
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
                    IsDateEditorExpanded = true
                };
            }
        }
    }
    public class SampleHealthRecordViewModel : HealthRecordViewModel
    {
        public SampleHealthRecordViewModel()
            : base(Mocks.hr)
        {
            IsDateEditorExpanded = true;
        }
    }
    public class SampleCardViewModel : CardViewModel
    {
        public SampleCardViewModel()
        {
        }
    }

    public class SampleDateOffsetViewModel : DateOffsetViewModel
    {
        public SampleDateOffsetViewModel()
            : base(Mocks.hr)
        {
        }
    }

    public class SampleHrListViewModel : HrListViewModel
    {
        public SampleHrListViewModel() :
            base(Mocks.course, (hr, info) => { }, (hios) => { })
        {
        }
    }

    public class SampleDoctorEditorViewModel : DoctorEditorViewModel
    {
        public SampleDoctorEditorViewModel()
            : base(Mocks.doc)
        {
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

    public class SampleIcdSelectorViewModel : IcdSelectorViewModel
    {
        public SampleIcdSelectorViewModel()
            : base("перикардит")
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

    public class Mocks
    {
        public static Patient pat = new Patient("Иванов", "Иван", year: 2000);
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