﻿using Diagnosis.Common;
using System.Windows;
using System.Windows.Input;

namespace Diagnosis.Common.Presentation.Behaviors
{
    public class ClipboardBehavior
    {
        public static readonly DependencyProperty ClipboardTargetProperty =
          DependencyProperty.RegisterAttached("ClipboardTarget", typeof(bool), typeof(ClipboardBehavior),
              new PropertyMetadata(false,
              (sender, e) =>
              {
                  if ((bool)e.NewValue)
                  {
                      var element = sender as FrameworkElement;
                      if (element == null) return;

                      OnAttached(element);
                  }
              }));

        public static bool GetClipboardTarget(UIElement element)
        {
            return (bool)element.GetValue(ClipboardTargetProperty);
        }

        public static void SetClipboardTarget(UIElement element, bool value)
        {
            element.SetValue(ClipboardTargetProperty, value);
        }

        public static readonly DependencyProperty OnlySelfProperty =
           DependencyProperty.RegisterAttached("OnlySelf", typeof(bool), typeof(ClipboardBehavior),
               new PropertyMetadata(false));

        public static bool GetOnlySelf(DependencyObject obj)
        {
            return (bool)obj.GetValue(OnlySelfProperty);
        }

        public static void SetOnlySelf(DependencyObject obj, bool value)
        {
            obj.SetValue(OnlySelfProperty, value);
        }

        protected static void OnAttached(FrameworkElement element)
        {
            CommandBinding CopyCommandBinding = new CommandBinding(
                ApplicationCommands.Copy,
                CopyCommandExecuted,
                CopyCommandCanExecute);
            element.CommandBindings.Add(CopyCommandBinding);

            CommandBinding CutCommandBinding = new CommandBinding(
                ApplicationCommands.Cut,
                CutCommandExecuted,
                CutCommandCanExecute);
            element.CommandBindings.Add(CutCommandBinding);

            CommandBinding PasteCommandBinding = new CommandBinding(
                ApplicationCommands.Paste,
                PasteCommandExecuted,
                PasteCommandCanExecute);
            element.CommandBindings.Add(PasteCommandBinding);
        }

        private static void CopyCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var vm = GetTargetVm(target, e);
            if (vm != null)
            {
                vm.Copy();
                e.Handled = true;
            }
        }

        private static void CopyCommandCanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void CutCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var vm = GetTargetVm(target, e);
            if (vm != null)
            {
                vm.Cut();
                e.Handled = true;
            }
        }

        private static void CutCommandCanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void PasteCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var vm = GetTargetVm(target, e);
            if (vm != null)
            {
                vm.Paste();
                e.Handled = true;
            }
        }

        private static void PasteCommandCanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            var vm = GetTargetVm(target, e);
            if (vm != null)
            {
                e.CanExecute = vm.CanPaste();
            }
        }

        private static IClipboardTarget GetTargetVm(object target, RoutedEventArgs e)
        {
            var fe = target as FrameworkElement;
            var vm = fe.DataContext as IClipboardTarget;
            if (GetOnlySelf(fe) && e.Source != target)
                return null;
            return vm;
        }
    }
}