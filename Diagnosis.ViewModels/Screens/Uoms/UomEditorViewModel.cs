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
using System.Collections.ObjectModel;

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

            Types = new List<UomType>(Session.QueryOver<UomType>().List());
            Formats = new ObservableCollection<UomFormat>(uom.Formats);

            (uom as IEditableObject).BeginEdit();
            Uom = new UomViewModel(uom);
            Uom.IsBase = false; // не делать новые единицы базовыми по умолчанию

            var uoms = Session.Query<Uom>()
               .ToList();
            Uom.PropertyChanged += (s, e) =>
            {
                if (UomValidator.TestExistingFor.Contains(e.PropertyName))
                {
                    TestExisting(Uom, uoms);
                }
                Uom.WasEdited = true;
            };

            TestExisting(Uom, uoms);

            Title = "Единица";
            HelpTopic = "edituom";
            WithHelpButton = false;
        }

        public override bool CanOk
        {
            get
            {
                return !Uom.HasExistingDescrAbbr && uom.IsValid();
            }
        }
        public List<UomType> Types { get; private set; }
        public ObservableCollection<UomFormat> Formats { get; private set; }

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
        /// Нельзя добавить единицу с таким же описанием или обозначением в группе.
        /// </summary>
        private void TestExisting(UomViewModel vm, IEnumerable<Uom> uoms)
        {
            vm.HasExistingDescrAbbr = uoms.Any(x =>
                (x.Description == uom.Description || x.Abbr == uom.Abbr) && x.Type == uom.Type && x != uom);
        }
        protected override void OnOk()
        {
            uom.Factor = Math.Log10(Uom.ValueInBase); // 1 мл = 1000 л, фактор 3
            uom.SetFormats(Formats.Where(x => x.IsValid()));

            (uom as IEditableObject).EndEdit();

            var toSave = new List<Uom>() { uom };
            // пересчет единиц типа на новую базу
            if (Uom.IsBase)
            {
                if (uom.Type.Rebase(uom))
                    toSave.AddRange(uom.Type.Uoms);
            }

            new Saver(Session).Save(toSave.ToArray());

            // force updating uoms colection in type
            Session.Refresh(uom.Type);
            uom.Type.RaiseOnPropertyChanged("Base");

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