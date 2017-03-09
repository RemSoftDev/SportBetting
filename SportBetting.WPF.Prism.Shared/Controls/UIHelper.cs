// Copyright (c) 2013 xmetropol.
// This code is distributed under the Microsoft Public License (Ms-PL).
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace SportBetting.WPF.Prism.Shared.Controls
{
  public static class UIHelper
  {
    #region Methods

    private static DependencyObject GetParent(DependencyObject dependencyObject)
    {
      var fre = dependencyObject as FrameworkElement;
      return VisualTreeHelper.GetParent(fre) ?? (fre != null ? fre.Parent : null);
    }

    private static IEnumerable<DependencyObject> VisualAncestorsInt(DependencyObject depObj, bool self)
    {
      if (depObj == null)
        yield break;

      if (self)
        yield return depObj;

      for (var parent = GetParent(depObj);
           parent != null;
           parent = GetParent(parent))
        yield return parent;
    }

    public static IEnumerable<DependencyObject> VisualAncestors(this DependencyObject depObj)
    {
      return VisualAncestorsInt(depObj, false);
    }

    public static IEnumerable<DependencyObject> VisualAncestorsAndSelf(this DependencyObject depObj)
    {
      return VisualAncestorsInt(depObj, true);
    }

    public static T FindVisualParent<T>(this DependencyObject child)
      where T : DependencyObject
    {
      return VisualAncestors(child).OfType<T>().FirstOrDefault();
    }

    public static IEnumerable<DependencyObject> VisualDescendants(this DependencyObject parent)
    {
      return VisualDescendantsInt(parent, false);
    }

    public static IEnumerable<DependencyObject> VisualDescendantsAndSelf(this DependencyObject parent)
    {
      return VisualDescendantsInt(parent, true);
    }

    private static IEnumerable<DependencyObject> VisualChildrenAndSelfInt(DependencyObject parent, bool self)
    {
      if (parent == null)
        yield break;

      if (self)
        yield return parent;

      var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
      for (var iChild = 0; iChild < childrenCount; iChild++)
        yield return VisualTreeHelper.GetChild(parent, iChild);
    }

    public static IEnumerable<DependencyObject> VisualChildren(this DependencyObject parent)
    {
      return VisualChildrenAndSelfInt(parent, false);
    }

    public static IEnumerable<DependencyObject> VisualChildrenAndSelf(this DependencyObject parent)
    {
      return VisualChildrenAndSelfInt(parent, true);
    }

    private static IEnumerable<DependencyObject> VisualDescendantsInt(DependencyObject parent, bool self)
    {
      if (parent == null)
        yield break;

      if (self)
        yield return parent;

      var queue = new Queue<DependencyObject>();
      queue.Enqueue(parent);
      do
      {
        var current = queue.Dequeue();

        yield return current;

        foreach (var child in VisualChildren(current))
          queue.Enqueue(child);

      } while (queue.Count > 0);
    }


    public static void ExecuteOnLayoutUpdate(this FrameworkElement fre, Action<FrameworkElement> action)
    {
      EventHandler handler = null;
      handler = delegate
                {
                  action(fre);
                  fre.LayoutUpdated -= handler;
                };

      fre.LayoutUpdated += handler;
    }

    public static Rect GetClientBox(this FrameworkElement fre)
    {
      return new Rect(0, 0, fre.ActualWidth, fre.ActualHeight);
    }

    public static Rect GetBoundingBox(this FrameworkElement element, FrameworkElement relativeTo)
    {
      return element.IsDescendantOf(relativeTo) ? element.TransformToVisual(relativeTo).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight)) : new Rect();
    }

    internal static IEnumerable<DependencyObject> VisualHitTest(Visual reference, Point point)
    {
      var result = new List<DependencyObject>();
      VisualTreeHelper.HitTest(reference, null, target =>
                                                 {
                                                   result.Add(target.VisualHit);
                                                   return HitTestResultBehavior.Continue;
                                                 }, new PointHitTestParameters(point));
      return result;
    }


    #endregion
  }
}
