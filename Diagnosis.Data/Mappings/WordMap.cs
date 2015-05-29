using Diagnosis.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Diagnosis.Data.Mappings.Client
{
    public class WordMapClient : ClassMapping<Word>
    {
        public WordMapClient()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(Length.WordTitle);
                m.UniqueKey(Names.Unique.WordTitle);
            });

            ManyToOne(x => x.Parent, m =>
            {
                m.Column(Names.Col.WordParent);
            });
            // только на клиенте
            ManyToOne(x => x.Uom, m =>
            {
                m.Column(Names.Id.Uom);
                m.NotNullable(false);
            });

            Set(x => x.Children, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Col.WordParent);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });

            Bag(x => x.HealthRecords, s =>
            {
                s.Table(Names.HrItem);
                s.Key(k =>
                {
                    k.Column(Names.Id.Word);
                });
                s.Inverse(true); // hr in owner of that relation, that prop is readonly
                s.Cascade(Cascade.None);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.ManyToMany(x =>
                {
                    x.Column(Names.Id.HealthRecord);
                    x.Class(typeof(HealthRecord));
                });
            });

            Set(x => x.VocabularyWords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Word);
                    //k.OnDelete(OnDeleteAction.Cascade);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
            Set(x => x.CritWords, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Id.Word);
                });
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Inverse(true);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });
        }
    }
}
namespace Diagnosis.Data.Mappings.Server
{
    public class WordMapServer : ClassMapping<Word>
    {
        public WordMapServer()
        {
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            Property(x => x.Title, m =>
            {
                m.NotNullable(true);
                m.Length(Length.WordTitle);
                m.UniqueKey(Names.Unique.WordTitle);
            });

            ManyToOne(x => x.Parent, m =>
            {
                m.Column(Names.Col.WordParent);
            });

            Set(x => x.Children, s =>
            {
                s.Key(k =>
                {
                    k.Column(Names.Col.WordParent);
                });
                s.Inverse(true);
                s.Cascade(Cascade.All | Cascade.DeleteOrphans);
                s.Access(Accessor.Field);
            }, r =>
            {
                r.OneToMany();
            });


        }
    }
}