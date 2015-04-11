using System;
using System.Linq;

namespace Diagnosis.Models
{
    public static class EntityExtensions
    {
        public static bool ShortIdsFromEnd;
        /// <summary>
        /// To unproxy lozy-loaded entity. http://sessionfactory.blogspot.ru/2010/08/hacking-lazy-loaded-inheritance.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T As<T, TId>(this EntityBase<TId> entity)
        {
            if (entity == null)
                return default(T);
            return (T)entity.Actual;
        }

        public static string ShortId<TId>(this EntityBase<TId> entity)
        {
            if (entity.Id is Guid)
                return string.Format("#{0}..", entity.Id.ToString().Substring(ShortIdsFromEnd ? 33 : 0, 3));
            else
                return string.Format("#{0}", entity.Id);
        }
    }
}