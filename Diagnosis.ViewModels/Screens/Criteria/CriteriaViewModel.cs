using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using EventAggregator;
using log4net;
using System;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public partial class CriteriaViewModel : ScreenBaseViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CriteriaViewModel));

        private static HierViewer<Estimator, CriteriaGroup, Criterion, ICrit> viewer;
        private Saver saver;
        private DialogViewModel _curEditor;
        private bool naviagationExpected;
        private EventMessageHandlersManager handlers;

        public CriteriaViewModel()
        {
            saver = new Saver(Session);
            viewer = new HierViewer<Estimator, CriteriaGroup, Criterion, ICrit>(
                cg => cg.Estimator,
                cr => cr.Group,
                e => e.CriteriaGroups,
                cg => cg.Criteria
                );
            Navigator = new CritNavigator(viewer, beforeInsert: CloseEditor); // first save editing crit
            Navigator.CurrentChanged += (s, e) =>
            {
                var c = e.arg != null ? (e.arg as CriteriaItemViewModel).Crit : null;
                ShowEditor(c);
            };
            Navigator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "CurrentTitle")
                {
                    Title = Navigator.CurrentTitle;
                }
            };

            var ests = EntityQuery<Estimator>.All(Session)();
            ests.ForEach(x =>
                Navigator.AddRootItemFor(x));

            handlers = new EventMessageHandlersManager(new EventMessageHandler[] {
                this.Subscribe(Event.DeleteCrit, (e) =>
                {
                    var crit = e.GetValue<ICrit>(MessageKeys.Crit);
                    DeleteCrit(crit);
                }),
                this.Subscribe(Event.EntityDeleted, (e) =>
                {
                    var entity = e.GetValue<IEntity>(MessageKeys.Entity);
                    if (entity is ICrit)
                        OnCritDeleted(entity as ICrit);
                }),
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
                    handlers.Dispose();
                    CloseEditor();
                    Navigator.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void DeleteCrit(ICrit crit)
        {
            if (crit is CriteriaGroup)
            {
                var crgr = crit as CriteriaGroup;
                crgr.Estimator.RemoveCriteriaGroup(crgr);
            }
            else if (crit is Criterion)
            {
                var cr = crit as Criterion;
                cr.Group.RemoveCriterion(cr);
            }
            saver.Delete(crit);
        }

        private void OnCritDeleted(ICrit crit)
        {
            viewer.RemoveFromHistory(crit);

            if (crit is Estimator)
            {
                Navigator.RemoveRoot(crit as Estimator);
            }
        }

        private void CloseEditor()
        {
            Contract.Ensures(CurrentEditor == null);

            if (CurrentEditor == null)
                return;

            if (CurrentEditor.DialogResult == null)
            {
                // сначала сохраним открытое, чтобы не сохранять инвалидное новое (а в карточке так можно)
                if (CurrentEditor.CanOk)
                    CurrentEditor.OkCommand.Execute(null);
                else
                    CurrentEditor.CancelCommand.Execute(null);
            }
            else
            {
                if (CurrentEditor.DialogResult == false)
                {
                    var crit = (CurrentEditor as ICritKeeper).Crit;
                    // убираем новую сущность, если ее нельзя сохранять
                    if (crit.IsTransient)
                    {
                        DeleteCrit(crit);
                        OnCritDeleted(crit);
                    }
                }

                CurrentEditor.Dispose();
                CurrentEditor = null;
                if (!naviagationExpected)
                {
                    // если закрываем редактор перед открытием нового, не обнулять current
                    Navigator.NavigateTo(null);
                }
            }
        }

        private void ShowEditor(ICrit crit)
        {
            if (CurrentEditor != null && (CurrentEditor as ICritKeeper).Crit == crit)
                return; // уже открыт

            naviagationExpected = crit != null;
            CloseEditor();
            naviagationExpected = false;

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
                CurrentEditor.OnDialogResult((r) => CloseEditor());
            }
        }
    }
}