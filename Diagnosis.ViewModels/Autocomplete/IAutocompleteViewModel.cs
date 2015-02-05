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

        ICommand EditCommand { get; }

        ICommand SendToSearchCommand { get; }

        IDropTarget DropHandler { get; }

        void AddAndEditTag(TagViewModel tag, bool up);
        void OnDrop(DragEventArgs e);
    }
}