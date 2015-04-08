using Diagnosis.Models.Validators;
using FluentValidation.Results;
using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Speciality : ValidatableEntity<Guid>, IDomainObject
    {
        public static Speciality Null = new Speciality("—");  // для врача без специальности

        private Iesi.Collections.Generic.ISet<Doctor> doctors = new HashedSet<Doctor>();
        private Iesi.Collections.Generic.ISet<SpecialityIcdBlocks> specialityIcdBlocks = new HashedSet<SpecialityIcdBlocks>();
        private Iesi.Collections.Generic.ISet<SpecialityVocabularies> specialityVocabularies = new HashedSet<SpecialityVocabularies>();
        private string _title;

        public Speciality(string title)
        {
            Contract.Requires(title != null);

            Title = title;
        }

        protected Speciality()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler BlocksChanged;
        public virtual event NotifyCollectionChangedEventHandler VocsChanged;

        public virtual string Title
        {
            get { return _title; }
            set
            {
                var filtered = value.Replace(Environment.NewLine, " ").Replace('\t', ' ').Trim();
                SetProperty(ref _title, filtered, () => Title);
            }
        }

        public virtual IEnumerable<IcdBlock> IcdBlocks
        {
            get
            {
                return specialityIcdBlocks
                        .Where(x => x.Speciality == this)
                        .Select(x => x.IcdBlock).OrderBy(x => x.Code);

            }
        }

        public virtual IEnumerable<Doctor> Doctors
        {
            get { return doctors; }
        }

        public virtual IEnumerable<Vocabulary> Vocabularies
        {
            get
            {
                return specialityVocabularies
                  .Where(x => x.Speciality == this)
                  .Select(x => x.Vocabulary);
            }
        }

        public virtual IEnumerable<SpecialityIcdBlocks> SpecialityIcdBlocks
        {
            get { return specialityIcdBlocks; }
        }
        public virtual IEnumerable<SpecialityVocabularies> SpecialityVocabularies
        {
            get { return specialityVocabularies; }
        }
        public virtual IcdBlock AddBlock(IcdBlock block)
        {
            if (!IcdBlocks.Contains(block))
            {
                var si = new SpecialityIcdBlocks(this, block);
                specialityIcdBlocks.Add(si);

                OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, block));
            }
            return block;
        }

        public virtual void RemoveBlock(IcdBlock block)
        {
            var si = specialityIcdBlocks.Where(x => x.IcdBlock == block).FirstOrDefault();
            if (si != null)
            {
                specialityIcdBlocks.Remove(si);
                OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, block));
            }
        }
        public virtual Vocabulary AddVoc(Vocabulary voc)
        {
            if (!Vocabularies.Contains(voc))
            {
                var si = new SpecialityVocabularies(this, voc);
                specialityVocabularies.Add(si);

                voc.AddSpec(this);
                OnVocsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, voc));
            }
            return voc;
        }

        public virtual void RemoveVoc(Vocabulary voc)
        {
            var si = specialityVocabularies.Where(x => x.Vocabulary == voc).FirstOrDefault();
            if (si != null)
            {
                specialityVocabularies.Remove(si);
                voc.RemoveSpec(this);
                OnVocsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, voc));
            }
        }
        public override string ToString()
        {
            return Title;
        }

        public override ValidationResult SelfValidate()
        {
            return new SpecialityValidator().Validate(this);
        }

        protected virtual void OnBlocksChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = BlocksChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
        protected virtual void OnVocsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = VocsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}