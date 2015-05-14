using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using NHibernate.Linq;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CriteriaViewModel : ScreenBaseViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CriteriaViewModel));

        private Saver saver;
        private static HierViewer<Estimator, CriteriaGroup, Criterion, ICrit> viewer;
        private DialogViewModel _curEditor;
        private EventMessageHandler handler;

        public CriteriaViewModel()
        {
            saver = new Saver(Session);
            viewer = new HierViewer<Estimator, CriteriaGroup, Criterion, ICrit>(
                cg => cg.Estimator,
                cr => cr.Group,
                e => e.CriteriaGroups,
                cg => cg.Criteria
                );
            Navigator = new CritNavigator(viewer, CloseEditor);
            Navigator.CurrentChanged += (s, e) =>
            {
                var c = e.arg as CriteriaItemViewModel;
                ShowEditor(c.Crit as ICrit);
            };
            Navigator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentTitle")
                {
                    Title = Navigator.CurrentTitle;
                }
            };

            var ests = Session.Query<Estimator>().ToList();
            ests.ForEach(x =>
                Navigator.AddRootItemFor(x));

            handler = this.Subscribe(Event.DeleteCrit, (e) =>
            {
                var crit = e.GetValue<ICrit>(MessageKeys.Crit);
                OnDeleteCrit(crit);
            });
            //var last = ests.LastOrDefault();
            //if (last != null)
            //{
            //    Open(last);
            //}
        }
        /// <summary>
        /// Создает и тут же вызывает Open(entity).
        /// </summary>
        public CriteriaViewModel(ICrit entity)
            : this()
        {
            Open(entity);
        }

        public CritNavigator Navigator { get; private set; }

        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseEditor();
                    var est = new Estimator();
                    Navigator.NavigateTo(est);
                });
            }
        }

        public DialogViewModel CurrentEditor
        {
            get
            {
                return _curEditor;
            }
            set
            {
                if (_curEditor != value)
                {
                    _curEditor = value;
                    OnPropertyChanged(() => CurrentEditor);
                }
            }
        }

        internal void Open(ICrit crit)
        {
            Contract.Requires(crit != null);
            logger.DebugFormat("open {0}", crit);

            Navigator.NavigateTo(crit);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                    CloseEditor();
                    Navigator.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void OnDeleteCrit(ICrit crit)
        {
            viewer.RemoveFromHistory(crit);

            if (crit is Estimator)
            {
                Navigator.RemoveRoot(crit as Estimator);
                saver.Delete(crit);
            }
            else if (crit is CriteriaGroup)
            {
                var crgr = crit as CriteriaGroup;
                crgr.Estimator.RemoveCriteriaGroup(crgr);
                saver.Save(viewer.OpenedRoot);
            }
            else if (crit is Criterion)
            {
                var cr = crit as Criterion;
                cr.Group.RemoveCriterion(cr);
                saver.Save(viewer.OpenedRoot);
            }
        }

        /// <summary>
        /// Close opened editor.
        /// </summary>
        private void CloseEditor()
        {
            if (CurrentEditor == null)
                return;

            if (CurrentEditor.DialogResult == null)
            {
                if (CurrentEditor.CanOk)
                    CurrentEditor.OkCommand.Execute(null);
                else
                    CurrentEditor.CancelCommand.Execute(null);
            }

            if (CurrentEditor == null)
                // если закрыли выше
                return;

            CurrentEditor.Dispose();
            CurrentEditor = null;
        }
        /// <summary>
        /// Close opened editor by setting dialog result.
        /// </summary>
        private void CloseEditor(bool result)
        {
            Contract.Requires(CurrentEditor != null);

            var crit = (CurrentEditor as ICritKeeper).Crit;
            CurrentEditor.Dispose();
            CurrentEditor = null;

            if (result == false)
            {
                // удаляем новую сущность, если ее нельзя сохранять
                if (crit.IsTransient)
                    OnDeleteCrit(crit);
            }
        }
        private void ShowEditor(ICrit crit)
        {
            if (CurrentEditor != null && (CurrentEditor as ICritKeeper).Crit == crit)
                return; // уже открыт

            CloseEditor();

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
            if (CurrentEditor != null)
            {
                CurrentEditor.OnDialogResult((r) => CloseEditor(r));
            }
        }
    }
}