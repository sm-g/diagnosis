using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Client.App.DesignData
{
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
        {
            this.doctor = Mocks.doc;
        }
    }

    public class SampleMeasureEditorViewModel : MeasureEditorViewModel
    {
        public SampleMeasureEditorViewModel()
            : base(new Measure(36, new Uom("C", 0, new UomType("температура"))) { Word = new Word("температура") })
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

    internal static class Mocks
    {
        public static Patient pat = new Patient("Иванов", "Иван", year: 2000);
        public static Doctor doc = new Doctor("Ivanov");
        public static Course course;
        public static Course course2;
        public static HrCategory[] cats = new[] {
            new HrCategory() { Name = "Жалоба" },
            new HrCategory() { Name = "История" },
            new HrCategory() { Name = "Осмотр" },
            new HrCategory() { Name = "Обследование" },
            new HrCategory() { Name = "Диагноз" },
            new HrCategory() { Name = "Лечение" },
            new HrCategory() { Name = "Не указано" },
        };

        public static HealthRecord hr;

        static Mocks()
        {
            hr.AddItems(new IHrItemObject[] { new Word("анемия"), new Word("впервые"), new Comment("без осложнений") });
            course = new Course(pat, doc)
            {
                Start = DateTime.Now
            };
            course2 = new Course(pat, doc)
            {
                Start = DateTime.Now.AddDays(-4),
                End = DateTime.Now
            };
            hr = new HealthRecord(course, doc)
            {
                Category = cats[0],
                FromMonth = 5,
            };
        }
    }
}