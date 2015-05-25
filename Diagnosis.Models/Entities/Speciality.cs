using Diagnosis.Common;
using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    public class Speciality : ValidatableEntity<Guid>, IDomainObject
    {
        public readonly static Speciality Null = new NullSpeciality();  // для врача без специальности

        private ISet<Doctor> doctors = new HashSet<Doctor>();
        private ISet<SpecialityIcdBlocks> specialityIcdBlocks = new HashSet<SpecialityIcdBlocks>();
        private ISet<SpecialityVocabularies> specialityVocabularies = new HashSet<SpecialityVocabularies>();
        private Many2ManyHelper<SpecialityVocabularies, Vocabulary> svHelper;
        private Many2ManyHelper<SpecialityIcdBlocks, IcdBlock> sbHelper;
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
                Contract.Requires(value != null);

                var filtered = value.Prettify().Truncate(Length.LongTitle);
                SetProperty(ref _title, filtered, () => Title);
            }
        }

        public virtual IEnumerable<IcdBlock> IcdBlocks
        {
            get
            {
                return SbHelper.Values;
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
                return SvHelper.Values;
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

        private Many2ManyHelper<SpecialityIcdBlocks, IcdBlock> SbHelper
        {
            get
            {
                if (sbHelper == null)
                {
                    sbHelper = new Many2ManyHelper<SpecialityIcdBlocks, IcdBlock>(
                        specialityIcdBlocks,
                        x => x.Speciality == this,
                        x => x.IcdBlock);
                }
                return sbHelper;
            }
        }
        private Many2ManyHelper<SpecialityVocabularies, Vocabulary> SvHelper
        {
            get
            {
                if (svHelper == null)
                {
                    svHelper = new Many2ManyHelper<SpecialityVocabularies, Vocabulary>(
                       specialityVocabularies,
                       x => x.Speciality == this,
                       x => x.Vocabulary);
                }
                return svHelper;
            }
        }

        public virtual IcdBlock AddBlock(IcdBlock block)
        {
            if (!IcdBlocks.Contains(block))
            {
                var si = new SpecialityIcdBlocks(this, block);
                SbHelper.Add(si);

                OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, block));
            }
            return block;
        }

        public virtual void RemoveBlock(IcdBlock block)
        {
            if (SbHelper.Remove(block))
                OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, block));
        }
        public virtual Vocabulary AddVoc(Vocabulary voc)
        {
            if (!Vocabularies.Contains(voc))
            {
                var sv = new SpecialityVocabularies(this, voc);
                SvHelper.Add(sv);

                voc.AddSpecVoc(sv);

                OnVocsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, voc));
            }
            return voc;
        }

        public virtual void RemoveVoc(Vocabulary voc)
        {
            if (SvHelper.Remove(voc))
            {
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
        private sealed class NullSpeciality : Speciality
        {
            public NullSpeciality()
                : base("—")
            {
            }
        }
    }
}