﻿using Diagnosis.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Diagnosis.Models;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

[assembly: InternalsVisibleTo("Tests")]

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

                var results = validatableEntity.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName ||
                            columnToPropertyMap != null &&
                            x.PropertyName == columnToPropertyMap.GetValueOrDefault(columnName))
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                return message != null ? message : string.Empty;
            }
        }
        #endregion

        private static bool? _isInDesignMode;
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