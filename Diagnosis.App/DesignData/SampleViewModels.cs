using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace Diagnosis.App.DesignData
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

    public class SampleDoctorEditorViewModel : DoctorEditorViewModel
    {
        public SampleDoctorEditorViewModel()
        {
            this.doctor = new Doctor("Петров", "Иван", "Иванович");
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
            : base(new Patient("Иванов", "Иван", year: 2000))
        {
        }
    }

    public class SampleShortHealthRecordViewModel : ShortHealthRecordViewModel
    {
        private static Patient pat = new Patient();
        private static Doctor doc = new Doctor("Ivanov");
        private static Course holder = new Course(pat, doc);

        private static HealthRecord hr = new HealthRecord(holder, doc)
        {
            Category = new HrCategory() { Name = "жалоба" },
            FromMonth = 5,
        };

        public SampleShortHealthRecordViewModel()
            : base(Mocks.hr)
        {
            SortingExtraInfo = hr.Category.Name;
        }
    }

    internal static class Mocks
    {
        public static Patient pat = new Patient();
        public static Doctor doc = new Doctor("Ivanov");
        public static Course holder = new Course(pat, doc);
        public static HrCategory[] cats = new[] {
        
        new HrCategory() { Name = "Жалоба" },
        new HrCategory() { Name = "История" },
        new HrCategory() { Name = "Осмотр" },
        new HrCategory() { Name = "Обследование" },
        new HrCategory() { Name = "Диагноз" },
        new HrCategory() { Name = "Лечение" },
        new HrCategory() { Name = "Не указано" },
        };



        public static HealthRecord hr = new HealthRecord(holder, doc)
          {
              Category = cats[0],
              FromMonth = 5,
          };

        static Mocks()
        {
            hr.AddItems(new IHrItemObject[] { new Word("анемия"), new Word("впервые"), new Comment("без осложнений") });
        }
    }
}