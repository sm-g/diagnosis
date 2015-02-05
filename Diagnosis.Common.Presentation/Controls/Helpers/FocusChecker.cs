using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Diagnosis.Common.Presentation.Controls
{
    public class FocusChecker
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(FocusChecker));
        public static bool IsLogicFocusOutside<T>(T obj) where T : UIElement
        {
            // TraceLogicalTree(obj);
            //  TraceVisualTree(obj);
            var win = Window.GetWindow(obj);
            //logger.DebugFormat("window {0}", win);
            if (win == null)
                return true;

            var kbElement = Keyboard.FocusedElement;
            var logicElement = FocusManager.GetFocusedElement(win);

            var kbFocusedOut = !obj.IsKeyboardFocusWithin;
            var logicFocusedOut = !(logicElement.IsChildOf(obj) || obj == logicElement);

            //logger.DebugFormat("focused: kb {0} logic {1}", kbElement, logicElement);
            //logger.DebugFormat("out? kb {0} logic {1}", kbFocusedOut, logicFocusedOut);
            //logger.DebugFormat("\n");

            return logicFocusedOut;
        }

        static void TraceVisualTree(DependencyObject node)
        {
            logger.Debug("========= VISUAL TREE FOR " + node.GetType().Name + " ============");
            while (node != null)
            {
                Console.WriteLine(node.GetType().Name + " has " +
                    (Window.GetWindow(node) == null ? "no " : String.Empty) +
                    "window.");

                node = VisualTreeHelper.GetParent(node);
            }
        }

        static void TraceLogicalTree(DependencyObject node)
        {
            logger.Debug("========= LOGICAL TREE FOR " + node.GetType().Name + " ============");
            while (node != null)
            {
                Console.WriteLine(node.GetType().Name + " has " +
                   (Window.GetWindow(node) == null ? "no " : String.Empty) +
                   "window.");

                node = LogicalTreeHelper.GetParent(node);
            }
        }
    }


}