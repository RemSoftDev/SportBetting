using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{
    public static class AppVisualTree
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search

                    if (frameworkElement != null && frameworkElement.Name == childName && frameworkElement is ScrollViewer && ((ScrollViewer)frameworkElement).ExtentHeight > 0 && ((ScrollViewer)frameworkElement).IsVisible)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                    else if (frameworkElement != null && frameworkElement.Name == childName && !(frameworkElement is ScrollViewer))
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    //if (frameworkElement != null && frameworkElement.Name == childName)
                    //{
                    //    // if the child's name is of the request name
                    //    foundChild = (T)child;
                    //    break;
                    //}
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

		public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				string controlName = child.GetValue(Control.NameProperty) as string;
				if (controlName == name)
				{
					return child as T;
				}
				else
				{
					T result = FindVisualChildByName<T>(child, name);
					if (result != null)
						return result;
				}
			}
			return null;
		}
    }
}
