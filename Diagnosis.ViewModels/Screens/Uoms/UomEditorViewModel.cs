using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using NHibernate.Linq;
using Diagnosis.Models.Validators;

namespace Diagnosis.ViewModels.Screens
{
    public class UomEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UomEditorViewModel));
        private readonly Uom uom;

        private UomViewModel _vm;

        public UomEditorViewModel(Uom uom)
        {
            Contract.Requires(uom != null);
            this.uom = uom;

            (uom as IEditableObject).BeginEdit();

            var uoms = Session.Query<Uom>()
               .ToList();
            Uom = new UomViewModel(uom);
            Uom.PropertyChanged += (s, e) =>
            {
                if (UomValidator.TestExistingFor.Contains(e.PropertyName))
                {
                    TestExisting(Uom, uoms);
                }
                Uom.WasEdited = true;
            };
            TestExisting(Uom, uoms);

            Types = new List<UomType>(Session.QueryOver<UomType>().List());

            Title = "Единица";
        }

        [Obsolete]
        public UomEditorViewModel()
        {
            this.uom = new Uom("kg", 1, new UomType("mass"));
            Uom = new UomViewModel(uom);
        }

        public override bool CanOk
        {
            get
            {
                return !Uom.HasExistingDescrAbbr && uom.IsValid();
            }
        }
        public List<UomType> Types { get; private set; }

        public UomViewModel Uom
        {
            get
            {
                return _vm;
            }
            set
            {
                if (_vm != value)
                {
                    _vm = value;
                    OnPropertyChanged(() => Uom);
                }
            }
        }
        /// <summary>
        /// Нельзя добавить единицу с таким же описанием / обозначением.
        /// </summary>
        private void TestExisting(UomViewModel vm, IEnumerable<Uom> uoms)
        {
            vm.HasExistingDescrAbbr = uoms.Any(x =>
                (x.Description == uom.Description || x.Abbr == uom.Abbr) && x != uom);
        }
        protected override void OnOk()
        {
            uom.Factor = Math.Log10(Uom.ValueInBase); // 1 мл = 1000 л, фактор 3
            if (Uom.ValueInBase == 1)
                Uom.IsBase = true;

            (uom as IEditableObject).EndEdit();

            var toSave = new List<Uom>() { uom };
            // пересчет единиц типа на новую базу
            if (Uom.IsBase)
            {
                if (uom.Type.Rebase(uom))
                    toSave = new List<Uom>(uom.Type.Uoms);
            }

            new Saver(Session).Save(toSave.ToArray());
            this.Send(Event.UomSaved, uom.AsParams(MessageKeys.Uom));
        }
        protected override void OnCancel()
        {
            (uom as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Uom.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}