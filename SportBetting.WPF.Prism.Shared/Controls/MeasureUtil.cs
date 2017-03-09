// Copyright (c) 2013 xmetropol.
// This code is distributed under the Microsoft Public License (Ms-PL).
// All rights reserved.

using System.Windows;

namespace SportBetting.WPF.Prism.Shared.Controls
{
  public static class MeasureUtil
  {
    public static double Height(this Thickness thickness)
    {
      return thickness.Top + thickness.Bottom;
    }

    public static double Width(this Thickness thickness)
    {
      return thickness.Left + thickness.Right;
    }
  }
}
