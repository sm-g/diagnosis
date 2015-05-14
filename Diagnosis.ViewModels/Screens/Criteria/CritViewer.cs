using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Хранит последние открытые сущности иерархии.
    ///
    /// AutoOpen:
    /// Если родительская сущность открыта повторно, открывает последнего открытого ребенка.
    ///
    /// При присвоении OpenedEntity сначала закрывается текущая открытая,
    /// затем меняется свойство OpenedEntity, потом открывается новая сущность.
    ///
    /// Перед открытием и после закрытия вызывается OpenedChanged.
    /// </summary>
    public class CritViewer : NotifyPropertyChangedBase
    {
        private Estimator _openedEstimator;
        private CriteriaGroup _openedCriteriaGroup;
        private Criterion _openedApp;

        private Dictionary<Estimator, CriteriaGroup> patCriteriaGroupMap;
        private Dictionary<CriteriaGroup, Criterion> CriteriaGroupAppMap;
        private static List<Estimator> EstimatorsOpenOrder = new List<Estimator>();

        public event EventHandler<OpeningEventArgs> OpenedChanged;

        public CritViewer()
        {
            patCriteriaGroupMap = new Dictionary<Estimator, CriteriaGroup>();
            CriteriaGroupAppMap = new Dictionary<CriteriaGroup, Criterion>();
            EstimatorsOpenOrder = new List<Estimator>(); // reset
        }

        public Estimator OpenedEstimator
        {
            get
            {
                return _openedEstimator;
            }
            internal set
            {
                if (_openedEstimator != value)
                {
                    if (_openedEstimator != null)
                    {
                        OnEstimatorClosed(_openedEstimator);
                    }
                    _openedEstimator = value;

                    OnPropertyChanged(() => OpenedEstimator);
                    if (value != null)
                    {
                        OnEstimatorOpened(value);
                        OnPropertyChanged(() => LastOpenedEstimator);
                    }
                }
            }
        }

        public CriteriaGroup OpenedCriteriaGroup
        {
            get
            {
                return _openedCriteriaGroup;
            }
            internal set
            {
                if (_openedCriteriaGroup != value)
                {
                    if (_openedCriteriaGroup != null)
                    {
                        OnCriteriaGroupClosed(_openedCriteriaGroup);
                    }

                    _openedCriteriaGroup = value;

                    OnPropertyChanged(() => OpenedCriteriaGroup);
                    if (value != null)
                    {
                        OnCriteriaGroupOpened(value);
                    }
                }
            }
        }

        public Criterion OpenedCriterion
        {
            get
            {
                return _openedApp;
            }
            internal set
            {
                if (_openedApp != value)
                {
                    if (_openedApp != null)
                    {
                        OnCriterionClosed(_openedApp);
                    }

                    _openedApp = value;

                    OnPropertyChanged(() => OpenedCriterion);
                    if (value != null)
                    {
                        OnCriterionOpened(value);
                    }
                }
            }
        }

        /// <summary>
        /// Открывать последнюю открытую дочернюю сущность при открытии родителя.
        /// </summary>
        public bool AutoOpen { get; set; }

        public static Estimator LastOpenedEstimator { get { return EstimatorsOpenOrder.LastOrDefault(); } }

        public Criterion GetLastOpenedFor(CriteriaGroup CriteriaGroup)
        {
            Criterion app;
            if (CriteriaGroupAppMap.TryGetValue(CriteriaGroup, out app))
                return app;
            return null;
        }

        public CriteriaGroup GetLastOpenedFor(Estimator Estimator)
        {
            CriteriaGroup CriteriaGroup;
            if (patCriteriaGroupMap.TryGetValue(Estimator, out CriteriaGroup))
                return CriteriaGroup;
            return null;
        }

        public ICrit GetLastOpenedFor(ICrit holder)
        {
            if (holder is Estimator)
                return GetLastOpenedFor(holder as Estimator);
            if (holder is CriteriaGroup)
                return GetLastOpenedFor(holder as CriteriaGroup);
            return null;
        }

        internal void Close(ICrit holder)
        {
            if (OpenedEstimator.Equals(holder))
            {
                OpenedEstimator = null;
            }
            else if (OpenedCriteriaGroup.Equals(holder))
            {
                OpenedCriteriaGroup = null;
            }
            else if (OpenedCriterion.Equals(holder))
            {
                OpenedCriterion = null;
            }
        }

        internal void CloseAll()
        {
            OpenedEstimator = null;
        }

        internal void OpenEstimator(Estimator Estimator)
        {
            OpenedEstimator = Estimator;
        }

        internal void OpenCriteriaGroup(CriteriaGroup CriteriaGroup)
        {
            OpenedEstimator = CriteriaGroup.Estimator;
            OpenedCriteriaGroup = CriteriaGroup;
        }

        internal void OpenCriterion(Criterion app)
        {
            OpenedEstimator = app.Group.Estimator;
            OpenedCriteriaGroup = app.Group;
            OpenedCriterion = app;
        }

        internal void Open(ICrit holder)
        {
            if (holder is Estimator)
            {
                OpenEstimator(holder as Estimator);
            }
            else if (holder is CriteriaGroup)
            {
                OpenCriteriaGroup(holder as CriteriaGroup);
            }
            else if (holder is Criterion)
            {
                OpenCriterion(holder as Criterion);
            }
        }

        internal void RemoveFromHistory(ICrit holder)
        {
            if (holder is Estimator)
            {
                patCriteriaGroupMap.Remove(holder as Estimator);
                EstimatorsOpenOrder.RemoveAll(x => x == holder as Estimator);
            }
            else if (holder is CriteriaGroup)
            {
                var p = patCriteriaGroupMap.FirstOrDefault(x => x.Value == holder as CriteriaGroup).Key;
                if (p != null)
                    patCriteriaGroupMap.Remove(p);
                CriteriaGroupAppMap.Remove(holder as CriteriaGroup);
            }
            else if (holder is Criterion)
            {
                var c = CriteriaGroupAppMap.FirstOrDefault(x => x.Value == holder as Criterion).Key;
                if (c != null)
                    CriteriaGroupAppMap.Remove(c);
            }
        }

        private void OnEstimatorOpened(Estimator Estimator)
        {
            var e = new OpeningEventArgs(Estimator, OpeningAction.Open);
            OnOpenedChanged(e);

            EstimatorsOpenOrder.Add(Estimator);

            if (AutoOpen)
            {
                CriteriaGroup CriteriaGroup = GetLastOpenedFor(Estimator);
                if (CriteriaGroup == null)
                {
                    OpenedCriteriaGroup = Estimator.CriteriaGroups
                        .LastOrDefault();
                }
                else
                {
                    OpenedCriteriaGroup = CriteriaGroup;
                }
            }
        }

        private void OnEstimatorClosed(Estimator Estimator)
        {
            OpenedCriteriaGroup = null;

            var e = new OpeningEventArgs(Estimator, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnCriteriaGroupOpened(CriteriaGroup CriteriaGroup)
        {
            Contract.Requires(OpenedEstimator == CriteriaGroup.Estimator);
            var e = new OpeningEventArgs(CriteriaGroup, OpeningAction.Open);
            OnOpenedChanged(e);

            patCriteriaGroupMap[OpenedEstimator] = CriteriaGroup;

            if (AutoOpen)
            {
                Criterion app = GetLastOpenedFor(CriteriaGroup);
                if (app == null)
                {
                    OpenedCriterion = CriteriaGroup.Criteria
                        .LastOrDefault();
                }
                else
                {
                    OpenedCriterion = app;
                }
            }
        }

        private void OnCriteriaGroupClosed(CriteriaGroup CriteriaGroup)
        {
            OpenedCriterion = null;

            var e = new OpeningEventArgs(CriteriaGroup, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        private void OnCriterionOpened(Criterion app)
        {
            Contract.Requires(OpenedCriteriaGroup == app.Group);
            var e = new OpeningEventArgs(app, OpeningAction.Open);
            OnOpenedChanged(e);

            CriteriaGroupAppMap[OpenedCriteriaGroup] = app;
        }

        private void OnCriterionClosed(Criterion app)
        {
            var e = new OpeningEventArgs(app, OpeningAction.Close);
            OnOpenedChanged(e);
        }

        protected virtual void OnOpenedChanged(OpeningEventArgs e)
        {
            var h = OpenedChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        [Serializable]
        public class OpeningEventArgs : EventArgs
        {
            public readonly OpeningAction action;
            public readonly object entity;

            [DebuggerStepThrough]
            public OpeningEventArgs(object entity, OpeningAction action)
            {
                this.action = action;
                this.entity = entity;
            }
        }

        public enum OpeningAction
        {
            Open, Close
        }
    }
}