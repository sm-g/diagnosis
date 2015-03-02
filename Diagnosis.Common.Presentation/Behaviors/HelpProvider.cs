using System.Windows;
using System.Windows.Input;

namespace Diagnosis.Common.Presentation.Behaviors
{
    /// <summary>
    /// Send ShowHelp with associated topic when F1 pressed.
    /// </summary>
    public class HelpProvider
    {
        private static CommandBinding cb = new CommandBinding(
                            ApplicationCommands.Help,
                            new ExecutedRoutedEventHandler(Executed),
                            new CanExecuteRoutedEventHandler(CanExecute));

        public static readonly DependencyProperty TopicProperty =
            DependencyProperty.RegisterAttached("Topic", typeof(string), typeof(HelpProvider),
            new FrameworkPropertyMetadata(null,
            (sender, e) =>
            {
                var element = sender as UIElement;
                if (element == null) return;

                if (e.OldValue == null && e.NewValue != null)
                {
                    element.CommandBindings.Add(cb);
                }
                else if (e.NewValue == null)
                {
                    element.CommandBindings.Remove(cb);
                }
            }));

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;

            if (HelpProvider.GetTopic(senderElement) != null)
                e.CanExecute = true;
        }

        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var topic = HelpProvider.GetTopic(sender as FrameworkElement);
            typeof(HelpProvider).Send(Event.ShowHelp, topic.AsParams(MessageKeys.String));
        }

        public static string GetTopic(UIElement element)
        {
            return (string)element.GetValue(TopicProperty);
        }

        public static void SetTopic(UIElement element, string topic)
        {
            element.SetValue(TopicProperty, topic);
        }
    }
}