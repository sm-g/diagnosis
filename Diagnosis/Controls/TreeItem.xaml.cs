﻿using Diagnosis.Helpers;
using Diagnosis.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diagnosis.Controls
{
    /// <summary>
    /// Interaction logic for TreeItem.xaml
    /// </summary>
    public partial class TreeItem : UserControl, IEditableItem
    {
        private bool isReady = true;
        private bool isEditing;

        public TreeItem()
        {
            InitializeComponent();
            isReady = true;

            search.ResultItemClicked += search_ResultItemClicked;
        }

        public void BeginEdit()
        {
            if (isReady)
            {
                isEditing = true;
                isReady = false;
                editor.Visibility = System.Windows.Visibility.Visible;
                titleEditor.Focus();
            }
        }

        internal void BeginSearch()
        {
            if (isReady)
            {
                isReady = false;
                search.DataContext = new SearchViewModel(this.DataContext as SymptomViewModel);
                search.Visibility = System.Windows.Visibility.Visible;
                search.Focus();
            }
        }

        internal void ToggleCheckedState()
        {
            (DataContext as SymptomViewModel).ToggleChecked();
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
            isReady = true;
        }

        private void EndSearch()
        {
            search.Visibility = System.Windows.Visibility.Collapsed;
            isReady = true;
        }

        private void editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusChecker.IsFocusOutsideDepObject(editor))
            {
                CommitChanges();
            }
        }

        private void search_KeyUp(object sender, KeyEventArgs e)
        {
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