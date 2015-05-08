using Diagnosis.Common;
using Diagnosis.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class CardItemViewModel : HierarchicalBase<CardItemViewModel>, IHolderKeeper
    {
        private readonly IHrsHolder holder;
        private bool _isHighlighted;
        private bool _showAppTime;

        public CardItemViewModel(IHrsHolder holder)
        {
            this.holder = holder;

            HolderVm = new HolderViewModel(holder, true);
            if (holder is Patient)
            {
                var patient = holder as Patient;
                var courseVMs = patient.GetOrderedCourses()
                       .Reverse()
                       .Select(i => new CardItemViewModel(i.Actual as Course))
                       .ToList();
                foreach (var item in courseVMs)
                {
                    Children.Add(item);
                }
                patient.CoursesChanged += nested_IHrsHolders_Changed;
            }
            else if (holder is Course)
            {
                var course = holder as Course;

                var appVMs = course.GetOrderedAppointments()
                    .Select(app => new CardItemViewModel(app.Actual as Appointment))
                    .ToList();

                foreach (var item in appVMs)
                {
                    Children.Add(item);
                }

                CorrectAppTimeVisibilty();

                course.AppointmentsChanged += nested_IHrsHolders_Changed;
            }

            (holder as INotifyPropertyChanged).PropertyChanged += holder_PropertyChanged;
        }

        public IHrsHolder Holder { get { return holder; } }

        public HolderViewModel HolderVm { get; private set; }

        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged(() => IsHighlighted);
                }
            }
        }

        public bool ShowAppTime
        {
            get
            {
                return _showAppTime;
            }
            set
            {
                if (_showAppTime != value)
                {
                    _showAppTime = value;
                    OnPropertyChanged(() => ShowAppTime);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.GetType(), Holder);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Holder is Patient)
                    {
                        (Holder as Patient).CoursesChanged -= nested_IHrsHolders_Changed;
                    }
                    if (Holder is Course)
                    {
                        (Holder as Course).AppointmentsChanged -= nested_IHrsHolders_Changed;
                    }
                    (Holder as INotifyPropertyChanged).PropertyChanged -= holder_PropertyChanged;

                    HolderVm.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Показываем время для осмотров, если есть осмотр той же даты в курсе.
        /// </summary>
        private void CorrectAppTimeVisibilty()
        {
            Contract.Requires(Holder is Course);

            // CardItem-осмотры, для которых нашлись другие c такой же датой
            var appItemsWithSameDates = (from app in Children
                                         group app by (app.Holder as Appointment).DateAndTime.Date into grs
                                         where grs.Count() > 1
                                         select grs).SelectMany(i => i).ToList();

            foreach (var app in Children)
            {
                app.ShowAppTime = appItemsWithSameDates.Contains(app);
            }
        }

        private void nested_IHrsHolders_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IHrsHolder item in e.NewItems)
                {
                    var vm = new CardItemViewModel(item);
                    Children.AddSorted(vm, x => x.Holder, reverse: item is Course);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (IHrsHolder item in e.OldItems)
                {
                    var vm = Children.Where(w => w.Holder == item).FirstOrDefault();
                    Children.Remove(vm);
                    vm.Dispose();
                }
            }

            if (sender is Course)
                CorrectAppTimeVisibilty();
        }

        private void holder_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Course || sender is Appointment)
                Parent.Children.Sort(vm => vm.Holder);
        }
    }
}