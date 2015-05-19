using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Autocomplete
{
    [ContractClassFor(typeof(IViewAutocompleteViewModel))]
    internal abstract class ContractForIViewAutocompleteViewModel : IViewAutocompleteViewModel
    {
        public bool CanCompleteOnLostFocus
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public TagViewModel EditingTag
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool InDragDrop
        {
            get { throw new NotImplementedException(); }
        }

        public TagViewModel LastTag
        {
            get { throw new NotImplementedException(); }
        }

        public bool ShowAltSuggestion
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool InDispose
        {
            get { throw new NotImplementedException(); }
        }

        public void CompleteOnLostFocus(TagViewModel tag)
        {
            Contract.Requires(tag != null);
        }

        public void StartEdit(TagViewModel tag)
        {
            throw new NotImplementedException();
        }

        public void StartEdit()
        {
            throw new NotImplementedException();
        }
    }

    [ContractClassFor(typeof(IQbAutocompleteViewModel))]
    internal abstract class ContractForIQbAutocompleteViewModel : IQbAutocompleteViewModel
    {
        public event EventHandler EntitiesChanged = delegate { };
        public event EventHandler<TagEventArgs> TagCompleted = delegate { };
        public event EventHandler<BoolEventArgs> InputEnded = delegate { };
        public event EventHandler ConfidencesChanged = delegate { };
        public event EventHandler CHiosChanged = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public INotifyCollectionChanged Tags
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsPopupOpen
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<ConfWithHio> GetCHIOs()
        {
            throw new NotImplementedException();
        }

        public void ReplaceTagsWith(IEnumerable<object> items)
        {
        }

        public IEnumerable<ConfWithHio> GetCHIOsOfCompleted()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TagViewModel AddTag(object tagOrContent = null, int index = -1, bool isLast = false)
        {
            throw new NotImplementedException();
        }


        public void CompleteTypings()
        {
            throw new NotImplementedException();
        }
    }

}
