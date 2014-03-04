﻿using Diagnosis.Helpers;
using Diagnosis.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TileItem.xaml
    /// </summary>
    public partial class TileItem : UserControl, IEditableItem
    {
        private bool isEditing;
        private bool isSearching;

        public TileItem()
        {
            InitializeComponent();

            search.ResultItemClicked += search_ResultItemClicked;
        }

        public void ToggleEditState()
        {
            if (!isEditing)
            {
                BeginEdit();
            }
            else
            {
                EndEdit();
            }
        }

        public void ToggleSearchState()
        {
            if (!isSearching)
            {
                BeginSearch();
            }
            else
            {
                EndSearch();
            }
        }

        public void BeginEdit()
        {
            isEditing = true;
            editor.Visibility = System.Windows.Visibility.Visible;
            titleEditor.Focus();
        }

        public void CommitChanges()
        {
            EndEdit();
        }

        public void RevertChanges()
        {
            titleEditor.Text = title.Text;
            EndEdit();
        }

        private void EndEdit()
        {
            isEditing = false;
            editor.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(root))
            {
                CommitChanges();
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Space)
            {
                (DataContext as SymptomViewModel).ToggleChecked();
            }
            else if (e.Key == Key.F2)
            {
                BeginEdit();
            }
            else if (e.Key == Key.Enter)
            {
                CommitChanges();
            }
            else if (e.Key == Key.Escape)
            {
                RevertChanges();
            }
            else if (e.Key == Key.Insert || e.Key == Key.F && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                BeginSearch();
            }
        }

        private void toggleEdit_Click(object sender, RoutedEventArgs e)
        {
            ToggleEditState();
        }

        private void toggleSearch_Click(object sender, RoutedEventArgs e)
        {
            ToggleSearchState();
        }

        private void BeginSearch()
        {
            isSearching = true;
            search.DataContext = new SearchViewModel(DataContext as SymptomViewModel);
            search.Visibility = System.Windows.Visibility.Visible;
            search.Focus();
        }

        private void EndSearch()
        {
            isSearching = false;
            search.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
            var searchVM = search.DataContext as SearchViewModel;
            var symptomVM = DataContext as SymptomViewModel;
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                EndSearch();
            }
            if (e.Key == Key.Enter)
            {
                OnResultItemSelected();
            }
        }

        private void search_ResultItemClicked(object sender, System.EventArgs e)
        {
            OnResultItemSelected();
        }

        private void OnResultItemSelected()
        {
            var searchVM = search.DataContext as SearchViewModel;

            searchVM.CheckSelectedItem();
            searchVM.Clear();
        }
    }
}