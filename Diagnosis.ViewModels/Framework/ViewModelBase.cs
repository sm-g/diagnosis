using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Diagnosis.ViewModels
{
    public abstract class ViewModelBase : DisposableBase, INotifyPropertyChanged, IDataErrorInfo
    {
        protected readonly static TaskFactory uiTaskFactory;
        private static bool? _isInDesignMode;
        private static AuthorityController _ac;

        protected AuthorityController AuthorityController { get { return _ac; } }

        static ViewModelBase()
        {
            if (IsInDesignMode)
            {
                var vshost = Process.GetProcesses().First(_process => _process.ProcessName.Contains("Diagnosis")).Modules[0].FileName;
                var solutionDir = new FileInfo(vshost).Directory.Parent.Parent.Parent;
                var libsDir = Path.Combine(solutionDir.FullName, "libs");

                AppDomain.CurrentDomain.AssemblyResolve += (s0, e) =>
                {
                    // File.AppendAllText("P:\\test.txt", e.RequestingAssembly.GetName().Name + '\n');
                    switch (e.RequestingAssembly.GetName().Name)
                    {
                        case "EventAggregator":
                            return Assembly.LoadFrom(Path.Combine(libsDir, "EventAggregator.dll"));
                    }
                    return null;
                };
            }
            else
            {
                var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                uiTaskFactory = new TaskFactory(uiScheduler);
            }
            _ac = AuthorityController.Default;
        }

        #region INotifyPropertyChanged Members

        public virtual event PropertyChangedEventHandler PropertyChanged;

        [DebuggerStepThrough]
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (string name in propertyNames)
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        [DebuggerStepThrough]
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("The expression is not a member access expression.", "propertyExpression");
            }
            string memberName = memberExpr.Member.Name;
            OnPropertyChanged(memberName);
        }

        #endregion INotifyPropertyChanged Members

        #region IDataErrorInfo

        protected IValidatable validatableEntity;
        protected Dictionary<string, string> columnToPropertyMap;

        public virtual string Error
        {
            get { return null; }
        }

        public virtual string this[string columnName]
        {
            get
            {
                if (validatableEntity == null)
                    return string.Empty;

                var exTestable = this as IExistTestable;
                if (exTestable != null && !exTestable.WasEdited)
                    return string.Empty;

                var results = validatableEntity.SelfValidate();

                var message = results.Errors
                    .Where(x => x.PropertyName == columnName ||
                            columnToPropertyMap != null &&
                            x.PropertyName == columnToPropertyMap.GetValueOrDefault(columnName))
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();

                if (exTestable != null && exTestable.HasExistingValue)
                    message = exTestable.ThisValueExistsMessage;

                return message;
            }
        }

        #endregion IDataErrorInfo

        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    _isInDesignMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
                }

                return _isInDesignMode.GetValueOrDefault();
            }
        }
    }

    public class VmBaseEventArgs : EventArgs
    {
        public readonly ViewModelBase vm;

        [DebuggerStepThrough]
        public VmBaseEventArgs(ViewModelBase vm)
        {
            this.vm = vm;
        }
    }
}