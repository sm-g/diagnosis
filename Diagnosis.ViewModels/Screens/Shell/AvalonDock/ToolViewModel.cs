using Diagnosis.Common;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Скрываемая панель инструментов.
    /// </summary>
    public class ToolViewModel : PaneViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ToolViewModel));
        private static List<ToolViewModel> vms = new List<ToolViewModel>();
        protected Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable anchorable;
        private bool _isVisible = true;

        static ToolViewModel()
        {
            typeof(ToolViewModel).Subscribe(Event.Shutdown, (e) =>
            {
                vms.ForEach(x =>
                {
                    // показываем скрытые панели перед сериализацией, чтобы они остались в xml
                    x.IsVisible = true;

                    // разворачиваем их.
                    // https://avalondock.codeplex.com/workitem/17319
                    // если оставить их свернутыми, созданный LayoutAnchorGroup (в ToggleAutoHide)
                    // останется пустым, и при след. сворачивании будет создан новый LayoutAnchorGroup
                    // который мешает TabNavigation

                    // чтобы вернуть свернутость после загрузки, можно
                    // хранить свернутость в настройках
                    // убирать пустые LayoutAnchorGroup в xml
                    x.SetIsAutoHidden(false);
                });
            });
        }

        public ToolViewModel()
        {
            vms.Add(this);
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    logger.DebugFormat("IsVisible {0} -> {1}", ContentId, value);

                    if (anchorable != null)
                    {
                        if (value)
                            anchorable.Show();
                        else
                            anchorable.Hide();
                    }

                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public void Activate()
        {
            IsVisible = true;
            SetIsAutoHidden(false);
            IsActive = true;
        }

        public override string ToString()
        {
            return "tool " + Title;
        }

        /// <summary>
        /// Вместо IsAutoHidden depprop, которого нет
        /// </summary>
        private void SetIsAutoHidden(bool willBeAutoHidden)
        {
            if (anchorable == null)
            {
                logger.Warn("Cannot set IsAutoHidden without anchorable");
                return;
            }

            logger.DebugFormat("{3}. IsAutoHidden {0}, IsHidden {1}, willBeAutoHidden {2}",
                        anchorable.IsAutoHidden, anchorable.IsHidden, willBeAutoHidden, this);
            if (willBeAutoHidden != anchorable.IsAutoHidden)
            {
                anchorable.ToggleAutoHide();
            }
        }

        /// <summary>
        /// Чтобы работал IsVisible и управлять IsAutoHidden
        /// </summary>
        internal void SetAnchorable(Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable anchorable)
        {
            Contract.Requires(anchorable != null);

            //logger.DebugFormat("{0} SetAnchorable {1}", this, anchorable);
            if (this.anchorable == null)
            {
                this.anchorable = anchorable;

                // скрываем как только появился anchorable
                if (!IsVisible)
                    anchorable.Hide();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                vms.Remove(this);
            }

            base.Dispose(disposing);
        }
    }
}