using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections;

namespace Diagnosis.Core
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region  INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

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

    }
}