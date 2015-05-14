using Diagnosis.Common;
using Diagnosis.Models;
using EventAggregator;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Diagnosis.Data.Sync
{
    internal class RHSpec : RH<Speciality>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(Doctor), typeof(SpecialityIcdBlocks), typeof(SpecialityVocabularies) }; } }

        protected override Expression<Func<TUpdate, Speciality>> GetGetterInner<TUpdate>()
        {
            if (typeof(Doctor) == typeof(TUpdate))
                return (e) => (e as Doctor).Speciality;
            else if (typeof(SpecialityIcdBlocks) == typeof(TUpdate))
                return (e) => (e as SpecialityIcdBlocks).Speciality;
            else if (typeof(SpecialityVocabularies) == typeof(TUpdate))
                return (e) => (e as SpecialityVocabularies).Speciality;
            return null;
        }

        protected override IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<Speciality, Speciality> replacing)
        {
            return UpdateInChild<Doctor>(session, replacing)
               .Union<IEntity>(UpdateInChild<SpecialityIcdBlocks>(session, replacing))
               .Union<IEntity>(UpdateInChild<SpecialityVocabularies>(session, replacing));
        }

        protected override Action<TUpdate, Speciality> GetSetterInner<TUpdate>()
        {
            if (typeof(Doctor) == typeof(TUpdate))
                return (e, x) => (e as Doctor).Speciality = x;
            else if (typeof(SpecialityIcdBlocks) == typeof(TUpdate))
                return (e, x) => (e as SpecialityIcdBlocks).Speciality = x;
            else if (typeof(SpecialityVocabularies) == typeof(TUpdate))
                return (e, x) => (e as SpecialityVocabularies).Speciality = x;

            return null;
        }

        public override Expression<Func<Speciality, bool>> EqualsByVal(Speciality x)
        {
            return (y) =>
                 (x as Speciality).Title == (y as Speciality).Title;
        }
    }

    internal class RHCat : RH<HrCategory>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(HealthRecord) }; } }

        protected override Expression<Func<TUpdate, HrCategory>> GetGetterInner<TUpdate>()
        {
            if (typeof(HealthRecord) == typeof(TUpdate))
                return (e) => (e as HealthRecord).Category;
            return null;
        }

        protected override IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<HrCategory, HrCategory> replacing)
        {
            return UpdateInChild<HealthRecord>(session, replacing);
        }

        protected override Action<TUpdate, HrCategory> GetSetterInner<TUpdate>()
        {
            if (typeof(HealthRecord) == typeof(TUpdate))
                return (e, x) => (e as HealthRecord).Category = x;
            return null;
        }

        public override Expression<Func<HrCategory, bool>> EqualsByVal(HrCategory x)
        {
            return (y) =>
                      (x as HrCategory).Title == (y as HrCategory).Title;
        }
    }

    internal class RHVoc : RH<Vocabulary>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(VocabularyWords), typeof(SpecialityVocabularies) }; } }

        protected override Expression<Func<TUpdate, Vocabulary>> GetGetterInner<TUpdate>()
        {
            if (typeof(VocabularyWords) == typeof(TUpdate))
                return (e) => (e as VocabularyWords).Vocabulary;
            else if (typeof(SpecialityVocabularies) == typeof(TUpdate))
                return (e) => (e as SpecialityVocabularies).Vocabulary;
            return null;
        }

        protected override IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<Vocabulary, Vocabulary> replacing)
        {
            return UpdateInChild<VocabularyWords>(session, replacing)
               .Union<IEntity>(UpdateInChild<SpecialityVocabularies>(session, replacing));
        }

        protected override Action<TUpdate, Vocabulary> GetSetterInner<TUpdate>()
        {
            if (typeof(VocabularyWords) == typeof(TUpdate))
                return (e, x) => (e as VocabularyWords).Vocabulary = x;
            else if (typeof(SpecialityVocabularies) == typeof(TUpdate))
                return (e, x) => (e as SpecialityVocabularies).Vocabulary = x;
            return null;
        }

        public override Expression<Func<Vocabulary, bool>> EqualsByVal(Vocabulary x)
        {
            return (y) =>
                     (x as Vocabulary).Title == (y as Vocabulary).Title;
        }
    }

    internal class RHSpecBlocks : RH<SpecialityIcdBlocks>
    {
        public override IEnumerable<Type> Parents { get { return new[] { typeof(Speciality), typeof(IcdBlock) }; } }

        public override Expression<Func<SpecialityIcdBlocks, bool>> EqualsByVal(SpecialityIcdBlocks x)
        {
            return (y) =>
                    (x as SpecialityIcdBlocks).IcdBlock == (y as SpecialityIcdBlocks).IcdBlock &&
                    (x as SpecialityIcdBlocks).Speciality.Title == (y as SpecialityIcdBlocks).Speciality.Title;
        }
    }

    internal class RHSpecVocs : RH<SpecialityVocabularies>
    {
        public override IEnumerable<Type> Parents { get { return new[] { typeof(Speciality), typeof(Vocabulary) }; } }

        public override Expression<Func<SpecialityVocabularies, bool>> EqualsByVal(SpecialityVocabularies x)
        {
            return (y) =>
                    (x as SpecialityVocabularies).Vocabulary.Title == (y as SpecialityVocabularies).Vocabulary.Title &&
                    (x as SpecialityVocabularies).Speciality.Title == (y as SpecialityVocabularies).Speciality.Title;
        }
    }

    internal class RHUomType : RH<UomType>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(Uom) }; } }

        public override Expression<Func<UomType, bool>> EqualsByVal(UomType x)
        {
            return (y) =>
                    (x as UomType).Title == (y as UomType).Title;
        }

        protected override Expression<Func<TUpdate, UomType>> GetGetterInner<TUpdate>()
        {
            if (typeof(Uom) == typeof(TUpdate))
                return (e) => (e as Uom).Type;
            return null;
        }

        protected override Action<TUpdate, UomType> GetSetterInner<TUpdate>()
        {
            if (typeof(Uom) == typeof(TUpdate))
                return (e, x) => { (e as Uom).Type = x; };
            return null;
        }

        protected override IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<UomType, UomType> replacing)
        {
            return UpdateInChild<Uom>(session, replacing);
        }
    }

    internal class RHUom : RH<Uom>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(HrItem), typeof(UomFormat), typeof(Word) }; } }

        public override IEnumerable<Type> Parents { get { return new[] { typeof(UomType) }; } }

        public override Expression<Func<Uom, bool>> EqualsByVal(Uom x)
        {
            return (y) =>
                    (x as Uom).Abbr == (y as Uom).Abbr &&
                    (x as Uom).Description == (y as Uom).Description &&
                    (x as Uom).Type.Title == (y as Uom).Type.Title;
        }

        protected override Expression<Func<TUpdate, Uom>> GetGetterInner<TUpdate>()
        {
            if (typeof(Word) == typeof(TUpdate))
                return (e) => (e as Word).Uom;
            else if (typeof(UomFormat) == typeof(TUpdate))
                return (e) => (e as UomFormat).Uom;
            else if (typeof(HrItem) == typeof(TUpdate))
                return (e) => (e as HrItem).Measure != null ? (e as HrItem).Measure.Uom : null;
            return null;
        }

        protected override Action<TUpdate, Uom> GetSetterInner<TUpdate>()
        {
            if (typeof(Word) == typeof(TUpdate))
                return (e, x) => (e as Word).Uom = x;
            else if (typeof(UomFormat) == typeof(TUpdate))
                return (e, x) => (e as UomFormat).Uom = x;
            else if (typeof(HrItem) == typeof(TUpdate))
                return (e, x) => { if ((e as HrItem).Measure != null) (e as HrItem).Measure.Uom = x; };
            return null;
        }

        protected override IEnumerable<IEntity> UpdateInChildsInner(ISession session, Dictionary<Uom, Uom> replacing)
        {
            return UpdateInChild<Word>(session, replacing)
                .Union<IEntity>(UpdateInChild<HrItem>(session, replacing))
                .Union(UpdateInChild<UomFormat>(session, replacing));
        }
    }

    internal class RHUomFormat : RH<UomFormat>
    {
        public override IEnumerable<Type> Parents { get { return new[] { typeof(Uom) }; } }

        public override Expression<Func<UomFormat, bool>> EqualsByVal(UomFormat x)
        {
            return (y) =>
                     (x as UomFormat).String == (y as UomFormat).String &&
                     (x as UomFormat).MeasureValue == (y as UomFormat).MeasureValue &&
                     (x as UomFormat).Uom.Abbr == (y as UomFormat).Uom.Abbr &&
                     (x as UomFormat).Uom.Description == (y as UomFormat).Uom.Description &&
                     (x as UomFormat).Uom.Type.Title == (y as UomFormat).Uom.Type.Title;
        }
    }
    internal class RHCriterion : RH<Criterion>
    {
        public override IEnumerable<Type> Parents { get { return new[] { typeof(CriteriaGroup) }; } }

        public override Expression<Func<Criterion, bool>> EqualsByVal(Criterion x)
        {
            return (y) =>
                     (x as Criterion).Code == (y as Criterion).Code;
        }
    }

    internal class RHCriteriaGroup : RH<CriteriaGroup>
    {
        public override IEnumerable<Type> Parents { get { return new[] { typeof(Estimator) }; } }
        public override IEnumerable<Type> Childs { get { return new[] { typeof(Criterion) }; } }

        public override Expression<Func<CriteriaGroup, bool>> EqualsByVal(CriteriaGroup x)
        {
            return (y) =>
                     x.Description == y.Description;
        }
    }
    internal class RHEstimator : RH<Estimator>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(CriteriaGroup) }; } }

        public override Expression<Func<Estimator, bool>> EqualsByVal(Estimator x)
        {
            return (y) =>
                     x.Description == y.Description;
        }
    }
    internal class RHWord : RH<Word>
    {
        public override IEnumerable<Type> Childs { get { return new[] { typeof(HrItem), typeof(VocabularyWords) }; } }
        public override IEnumerable<Type> Parents { get { return new[] { typeof(Uom) }; } }

        public override Expression<Func<Word, bool>> EqualsByVal(Word x)
        {
            return (y) =>
                     x.Title == y.Title;
        }
    }
    class RHDummy : RH<IEntity>
    {

        public override Expression<Func<IEntity, bool>> EqualsByVal(IEntity x)
        {
            return (y) => false;
        }
    }
}