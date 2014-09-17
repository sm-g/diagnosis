using System;
using System.Collections;
using System.Collections.Generic;
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
        public static T As<T>(this EntityBase entity) where T : EntityBase
        {
            if (entity == null)
                return null;
            return (T)entity.Actual;
        }
    }
}
