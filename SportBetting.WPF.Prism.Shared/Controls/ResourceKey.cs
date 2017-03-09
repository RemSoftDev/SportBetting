// Copyright (c) 2013 xmetropol.
// This code is distributed under the Microsoft Public License (Ms-PL).
// All rights reserved.

using System;
using System.Windows.Markup;

namespace SportBetting.WPF.Prism.Shared.Controls
{
  public class ResourceKey : MarkupExtension
  {
    public object Value { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return Value;
    }
  }
}
