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

        private IList<IcdBlock> icdBlocks = new List<IcdBlock>(); // many-2-many bag
        private Iesi.Collections.Generic.ISet<Doctor> doctors = new HashedSet<Doctor>();
        private Iesi.Collections.Generic.ISet<SpecialityIcdBlocks> specialityIcdBlocks = new HashedSet<SpecialityIcdBlocks>();
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
            get { return icdBlocks.OrderBy(x => x.Code); }
        }

        public virtual IEnumerable<Doctor> Doctors
        {
            get { return doctors; }
        }

        public virtual IEnumerable<SpecialityIcdBlocks> SpecialityIcdBlocks
        {
            get { return specialityIcdBlocks; }
        }
        public virtual IcdBlock AddBlock(IcdBlock block)
        {
            icdBlocks.Add(block);
            OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, block));

            return block;
        }

        public virtual void RemoveBlock(IcdBlock block)
        {
            if (icdBlocks.Remove(block))
                OnBlocksChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, block));
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
    }
}