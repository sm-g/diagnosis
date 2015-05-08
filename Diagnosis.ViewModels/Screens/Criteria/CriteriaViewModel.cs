using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using NHibernate.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CriteriaViewModel : ScreenBaseViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CriteriaViewModel));

        private Saver saver;

        private DialogViewModel _cur;

        public CriteriaViewModel()
        {
            if (IsInDesignMode) return;

            saver = new Saver(Session);

            TopItems = new ObservableCollection<CriteriaItemViewModel>();

            var ests = Session.Query<Estimator>().ToList();
            ests.ForEach(x =>
                TopItems.Add(new CriteriaItemViewModel(x)));

        }

        /// <summary>
        /// Создает и тут же вызывает Open(entity).
        /// </summary>
        public CriteriaViewModel(ICrit entity)
            : this()
        {
            Open(entity);
        }

        public ObservableCollection<CriteriaItemViewModel> TopItems
        {
            get;
            private set;
        }

        public DialogViewModel CurrentEditor
        {
            get
            {
                return _cur;
            }
            set
            {
                if (_cur != value)
                {
                    _cur = value;
                    if (value != null)
                    {
                        // закрываем редактор
                        value.OnDialogResult((r) =>
                            CurrentEditor = null);
                    }
                    OnPropertyChanged(() => CurrentEditor);
                }
            }
        }

        internal void Open(ICrit crit)
        {
            Contract.Requires(crit != null);
            logger.DebugFormat("open {0}", crit);

            if (crit is Estimator)
            {
                CurrentEditor = new EstimatorEditorViewModel(crit as Estimator);
            }
            else if (crit is CriteriaGroup)
            {
                CurrentEditor = new CriteriaGroupEditorViewModel(crit as CriteriaGroup);
            }
            else if (crit is Criterion)
            {
                CurrentEditor = new CriterionEditorViewModel(crit as Criterion);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}