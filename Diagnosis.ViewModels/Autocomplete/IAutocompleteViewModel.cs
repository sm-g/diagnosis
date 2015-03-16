using GongSolutions.Wpf.DragDrop;
using System;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Autocomplete
{
    public interface IAutocompleteViewModel
    {
        TagViewModel SelectedTag { get; set; }

        bool SingleTag { get; }

        bool WithConvert { get; }

        bool WithSendToSearch { get; }
        bool WithConfidence { get; }
        
        ICommand EditCommand { get; }

        ICommand SendToSearchCommand { get; }
        ICommand ToggleConfidenceCommand { get; }


        void AddAndEditTag(TagViewModel tag, bool up);
        void OnDrop(DragEventArgs e);
    }
}