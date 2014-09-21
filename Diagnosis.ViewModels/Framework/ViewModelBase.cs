using Diagnosis.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Diagnosis.ViewModels
{
    public abstract class ViewModelBase : DisposableBase, INotifyPropertyChanged, IDataErrorInfo
    {
        #region  INotifyPropertyChanged Members

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
        #endregion

        #region IDataErrorInfo
        public virtual string Error
        {
            get { return null; }
        }

        public virtual string this[string columnName]
        {
            get { return null; }
        }
        #endregion
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