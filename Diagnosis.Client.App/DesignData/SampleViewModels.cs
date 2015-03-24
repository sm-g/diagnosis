using Diagnosis.Models;
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
            //Autocomplete = new AutocompleteViewModel(
            //    new Recognizer(Session) { OnlyWords = true, AddNotPersistedToSuggestions = false },
            //    AutocompleteViewModel.OptionsMode.Search,
            //    Mocks.hr.HrItems.Select(x => x.CHIO));

            ControlsVisible = true;
            AllWords = true;
        }
    }

    public class SampleTagViewModel : TagViewModel
    {
        public SampleTagViewModel()
            : base(new Mock<IAutocompleteViewModel>().Object)
        {
            Blank = new Comment("query");
            IsDraggable = true;
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

    public class SampleCardViewModel : CardViewModel
    {
        public SampleCardViewModel()
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

    public class SampleMeasureEditorViewModel : MeasureEditorViewModel
    {
        public SampleMeasureEditorViewModel()
            : base(new Measure(36, Mocks.uom) { Word = Mocks.word })
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
            SortingExtraInfo = Mocks.hr.Category.Name;
        }
    }

    public class Mocks
    {
        public static Patient pat = new Patient("Иванов", "Иван", year: 2000);
        public static Doctor doc = new Doctor("Ivanov", "Ivan");
        public static Course course = new Course(pat, doc)
        {
            Start = DateTime.Now
        };
        public static Course course2 = new Course(pat, doc)
        {
            Start = DateTime.Now.AddDays(-4),
            End = DateTime.Now
        };
        public static Word word = new Word("анемия");
        public static Word word2 = new Word("впервые");
        public static UomType uomType = new UomType("температура", 1);
        public static Uom uom = new Uom("C", 36, uomType);

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
                FromMonth = 5,
            };
            hr.AddItems(new IHrItemObject[] { word, word2, new Comment("без осложнений") });
        }
    }
}