using Diagnosis.Common;
using Diagnosis.Models;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Autocomplete
{
    public interface IViewAutocompleteViewModel
    {
        bool CanCompleteOnLostFocus { get; set; }
        TagViewModel EditingTag { get; set; }
        bool InDragDrop { get; }
        TagViewModel LastTag { get; }
        bool ShowAltSuggestion { get; set; }
        bool InDispose { get; }

        void CompleteOnLostFocus(TagViewModel tag);
        void StartEdit(TagViewModel tag);
        void StartEdit();
    }

    public interface ITagParentAutocomplete
    {
        bool SingleTag { get; }
        bool WithSendToSearch { get; }
        bool WithConfidence { get; }
        bool WithConvert { get; }

        ICommand EditCommand { get; }
        ICommand SendToSearchCommand { get; }
        ICommand ToggleConfidenceCommand { get; }


        void AddTagNearAndEdit(TagViewModel tag, bool up);
        bool WithConvertTo(BlankType type);
        void OnDrop(DragEventArgs e);
    }

    public interface IHrEditorAutocomplete : ITagsTrackableAutocomplete, IClipboardTarget, IDisposable
    {
        ICommand DeleteCommand { get; }

        ICommand SendToSearchCommand { get; }

        ICommand ToggleSuggestionModeCommand { get; }

        bool AddQueryToSuggestions { get; set; }

        void CompleteTypings();
        void StartEdit();
        void AddFromEditor(BlankType type, int index = -1);
    }
    public interface IQbAutocompleteViewModel : ITagsTrackableAutocomplete, IDisposable, INotifyPropertyChanged
    {
        INotifyCollectionChanged Tags { get; }
        bool IsEmpty { get; }
        void ReplaceTagsWith(IEnumerable<object> items);
    }
    public interface ITagsTrackableAutocomplete
    {
        event EventHandler EntitiesChanged;
        event EventHandler<TagEventArgs> TagCompleted;
        event EventHandler<BoolEventArgs> InputEnded;
        event EventHandler ConfidencesChanged;

        IEnumerable<ConfindenceHrItemObject> GetCHIOs();
        IEnumerable<ConfindenceHrItemObject> GetCHIOsOfCompleted();
    }
}