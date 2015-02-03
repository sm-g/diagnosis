using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Diagnosis.Common.Controls
{
    public static class ItemCollectionExtensions
    {
        /// <summary>
        /// Returns index of selected visible item in container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns>Index or -1 if no item selected.</returns>
        public static int SelectedIndex(this ItemsControl container)
        {
            int selectedIndex = 0;
            if (GetSelectedItemIndex(container, ref selectedIndex))
            {
                return selectedIndex;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns object of searchIndex in parentContainer.
        /// </summary>
        /// <param name="parentContainer"></param>
        /// <param name="searchIndex"></param>
        /// <returns>Found object or null.</returns>
        public static object FindByIndex(this ItemsControl parentContainer, int searchIndex)
        {
            int index = 0;
            if (searchIndex < 0)
            {
                return null;
            }
            return FindInItemsControl(parentContainer, ref index, searchIndex);
        }

        /// <summary>
        /// Returns number of objects in parentContainer.
        /// </summary>
        public static int Count(this ItemsControl parentContainer)
        {
            return CountItemsControl(parentContainer);
        }

        private static bool GetSelectedItemIndex(ItemsControl parentContainer, ref int index)
        {
            foreach (var item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null)
                {
                    if (currentContainer.IsSelected && currentContainer.IsVisible)
                        return true;
                    index++;

                    if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        // If the sub containers of current item is ready, we can directly go to the next
                        // iteration to expand them.

                        if (GetSelectedItemIndex(currentContainer, ref index))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static object FindInItemsControl(ItemsControl parentContainer, ref int index, int searchIndex)
        {
            foreach (var item in parentContainer.Items)
            {
                ItemsControl currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as ItemsControl;
                if (currentContainer != null)
                {
                    if (index == searchIndex)
                    {
                        return item;
                    }

                    index++;

                    var inner = FindInItemsControl(currentContainer, ref index, searchIndex);
                    if (inner != null)
                    {
                        return inner;
                    }
                }
            }
            return null;
        }

        private static int CountItemsControl(ItemsControl parentContainer)
        {
            int counter = parentContainer.Items.Count;
            foreach (var item in parentContainer.Items)
            {
                ItemsControl currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as ItemsControl;
                if (currentContainer != null)
                {
                    counter += CountItemsControl(currentContainer);
                }
            }
            return counter;
        }
    }
}