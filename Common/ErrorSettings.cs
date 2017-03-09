using System;
using System.Windows;
using SportRadar.Common.Enums;

namespace SportRadar.Common
{
    public class ErrorSettings
    {
        public ErrorLevel ErrorLevel { get; set; }

        public int AddCounterSeconds { get; set; }

        public EventHandler OkClick { get; set; }

        public bool CreateButtonEvent { get; set; }

        public bool HideButtons { get; set; }

        public Visibility WarningVisibility { get; set; }

        public TextAlignment TextAligment { get; set; }

    }
}