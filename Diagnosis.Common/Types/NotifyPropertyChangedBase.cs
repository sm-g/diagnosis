﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Diagnosis.Common.Types
{
    [Serializable]
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region  INotifyPropertyChanged Members
        [field: NonSerialized]
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

        public virtual void RaiseOnPropertyChanged(string name)
        {
            OnPropertyChanged(name);
        }
        public virtual void RaiseOnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            OnPropertyChanged(propertyExpression);
        }
    }
}