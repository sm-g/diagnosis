using Diagnosis.Core;
using PixelMEDIA.PixelCore.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Diagnosis.Models
{

    public static class EntityHelpers
    {
        /// <summary>
        /// To unproxy lozy-loaded entity. http://sessionfactory.blogspot.ru/2010/08/hacking-lazy-loaded-inheritance.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T As<T>(this EntityBase entity)
        {
            if (entity == null)
                return default(T);
            return (T)entity.Actual;
        }
    }

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

        public EditableObjectHelper(IEditableObject master)
        {
            if (master == null)
                throw new ArgumentNullException("master");

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
        public void Edit<T>(Expression<Func<T>> propertyExpression)
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

                    if (propValuePair.Item2 is EntityBase)
                    {
                        value = (propValuePair.Item2 as EntityBase).As<T>();  // unproxy
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