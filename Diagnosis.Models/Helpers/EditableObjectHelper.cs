using Diagnosis.Common;
using PixelMEDIA.PixelCore.Helpers;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Diagnosis.Models
{
    public class EditableObjectHelper
    {
        private object _syncRoot = new object();
        private IEditableObject _master;
        private IDictionary _originalValues;
        private bool _inEdit;
        private bool _inOriginalValuesReset;

        public bool InEdit
        {
            get { return _inEdit; }
        }

        public bool InOriginalValuesReset
        {
            get { return _inOriginalValuesReset; }
        }

        [Browsable(false)]
        protected IDictionary OriginalValues
        {
            get
            {
                if (_originalValues == null)
                {
                    _originalValues = new Hashtable();
                }
                return _originalValues;
            }
        }

        internal EditableObjectHelper(IEditableObject master)
        {
            Contract.Requires(master != null); ;

            _master = master;
        }

        public void BeginEdit()
        {
            lock (_syncRoot)
            {
                if (_inEdit)
                    return;

                _inEdit = true;
            }
        }

        /// <summary>
        /// Сохраняет значение свойства, каким оно было до вызова BeginEdit, один раз
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Edit(string propertyName, object value)
        {
            lock (_syncRoot)
            {
                if (_inOriginalValuesReset)
                    return;

                if (_inEdit)
                {
                    if (!OriginalValues.Contains(propertyName))
                    {
                        OriginalValues.Add(
                            propertyName,
                            value);
                    }
                }
            }
        }

        /// <summary>
        /// Сохраняет значение свойства, каким оно было до вызова BeginEdit, один раз
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        public void Edit<T, TId>(Expression<Func<T>> propertyExpression)
        {
            lock (_syncRoot)
            {
                if (_inOriginalValuesReset)
                    return;

                if (_inEdit)
                {
                    var propValuePair = ExpressionHelper.GetPropertyNameAndValue(propertyExpression);
                    var propertyName = propValuePair.Item1;
                    T value;

                    if (propValuePair.Item2 is EntityBase<TId>)
                    {
                        value = (T)(propValuePair.Item2 as EntityBase<TId>).Actual;  // unproxy
                    }
                    else
                    {
                        value = propValuePair.Item2;
                    }

                    if (!OriginalValues.Contains(propertyName))
                    {
                        OriginalValues.Add(
                            propertyName,
                            value);
                    }
                }
            }
        }

        public void CancelEdit()
        {
            lock (_syncRoot)
            {
                if (!_inEdit)
                    return;

                try
                {
                    _inOriginalValuesReset = true;

                    foreach (DictionaryEntry entry in OriginalValues)
                    {
                        ReflectionHelper.SetPropertyValue(
                            _master, (string)entry.Key, entry.Value);
                    }
                }
                finally
                {
                    _inOriginalValuesReset = false;
                }

                _inEdit = false;

                OriginalValues.Clear();
            }
        }

        public void EndEdit()
        {
            lock (_syncRoot)
            {
                if (!_inEdit)
                    return;

                _inEdit = false;

                OriginalValues.Clear();
            }
        }
    }
}