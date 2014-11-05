namespace Diagnosis.Models
{
    public static class EntityExtensions
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
}