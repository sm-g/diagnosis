using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Diagnosis.Data.Sync
{
    /// <summary>
    /// Relation helper
    /// </summary>
    public interface IRH : IComparable<Type>
    {
    }

    public class RHFactory
    {
        private static Dictionary<Type, IRH> dict = new Dictionary<Type, IRH>();

        static RHFactory()
        {
            dict.Add(typeof(HrCategory), new RHCat());
            dict.Add(typeof(UomFormat), new RHUomFormat());
            dict.Add(typeof(UomType), new RHUomType());
            dict.Add(typeof(Uom), new RHUom());
            dict.Add(typeof(Speciality), new RHSpec());
            dict.Add(typeof(SpecialityIcdBlocks), new RHSpecBlocks());
            dict.Add(typeof(SpecialityVocabularies), new RHSpecVocs());
            dict.Add(typeof(Vocabulary), new RHVoc());
            dict.Add(typeof(Criterion), new RHCriterion());
            dict.Add(typeof(CriteriaGroup), new RHCriteriaGroup());
            dict.Add(typeof(Estimator), new RHEstimator());
            dict.Add(typeof(Word), new RHWord());
        }

        public static RH<T> Create<T>()
        {
            IRH rh;
            if (dict.TryGetValue(typeof(T), out rh))
                return rh as RH<T>;
            throw new NotImplementedException("No rh factory for type");
        }
        public static IRH Create(Type type)
        {
            IRH rh;
            if (dict.TryGetValue(type, out rh))
                return rh;
            return new RHDummy() as IRH;
        }
    }

    /// <summary>
    /// Отношения child-parent для клиента.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RH<T> : IRH
    {
        public virtual IEnumerable<Type> Childs { get { return Enumerable.Empty<Type>(); } }

        public virtual IEnumerable<Type> Parents { get { return Enumerable.Empty<Type>(); } }

        /// <summary>
        /// Parents "greater" than children, so after OrderBy children proceeded first
        /// </summary>
        public int CompareTo(Type other)
        {
            if (Childs.Contains(other))
                return 1;
            if (Parents.Contains(other))
                return -1;
            return 0;
        }
        /// <summary>
        /// Меняет поле во всех дочерних сущностях и сохраняет их.
        /// </summary>
        public void UpdateInChilds(ISession session, Dictionary<T, T> replacing)
        {
            var toUpdate = UpdateInChildsInner(session, replacing);
            // сохраняем обновленных детей
            new Saver(session).Save(toUpdate.ToArray());
        }

        /// <summary>
        /// Выражение для проверки равенства двух сущностей по значению, несмотря на разные ID.
        /// </summary>
        public abstract Expression<Func<T, bool>> EqualsByVal(T x);

        /// <summary>
        /// Геттер свойства для обновления
        /// </summary>
        protected virtual Expression<Func<TUpdate, T>> GetGetterInner<TUpdate>()
        {
            return null;
        }

        /// <summary>
        /// Сеттер свойства для обновления
        /// </summary>
        protected virtual Action<TUpdate, T> GetSetterInner<TUpdate>()
        {
            return null;
        }
        /// <summary>
        /// Меняет поле во всех дочерних сущностях.
        /// </summary>
        protected virtual IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<T, T> replacing)
        {
            return Enumerable.Empty<IEntity>();
        }

        /// <summary>
        ///  Меняет поле в сущностях для обновления.
        /// </summary>
        /// <typeparam name="TUpdate">Тип сущности для обновления</typeparam>
        /// <param name="toReplace">Сущности для замены, значения { oldEntity, newEntity }</param>
        /// <returns></returns>
        protected IEnumerable<TUpdate> UpdateInChild<TUpdate>(ISession session, Dictionary<T, T> toReplace)
            where TUpdate : IEntity
        {
            Contract.Requires(Childs.Any());

            var selector = GetGetter<TUpdate>();
            var setter = GetSetter<TUpdate>();

            var olds = toReplace.Keys;

            // http://stackoverflow.com/a/1069820/3009578
            // from TUpdate e
            // where e in olds

            ParameterExpression p = selector.Parameters.Single();

            IEnumerable<Expression> equals = olds.Select(value =>
               (Expression)Expression.Equal(selector.Body,
                    Expression.Constant(value, typeof(T))));

            Expression body = equals.Aggregate((accumulate, equal) =>
                Expression.Or(accumulate, equal));

            var l = Expression.Lambda<Func<TUpdate, bool>>(body, p);

            var toUpdate = session
                .Query<TUpdate>()
                .Where(l)
                .ToList();

            var compiled = selector.Compile();
            toUpdate.ForEach(x => setter(x, toReplace[compiled(x)]));
            return toUpdate;
        }

        private Expression<Func<TUpdate, T>> GetGetter<TUpdate>()
        {
            if (!Childs.Contains(typeof(TUpdate)))
            {
                throw new ArgumentException("{0} is not child of {1}".FormatStr(typeof(TUpdate), typeof(T)));
            }
            var s = GetGetterInner<TUpdate>();
            if (s == null)
            {
                throw new NotImplementedException();
            }
            return s;
        }

        private Action<TUpdate, T> GetSetter<TUpdate>()
        {
            if (!Childs.Contains(typeof(TUpdate)))
            {
                throw new ArgumentException("{0} is not child of {1}".FormatStr(typeof(TUpdate), typeof(T)));
            }
            var s = GetSetterInner<TUpdate>();
            if (s == null)
            {
                throw new NotImplementedException();
            }
            return s;
        }
    }
}