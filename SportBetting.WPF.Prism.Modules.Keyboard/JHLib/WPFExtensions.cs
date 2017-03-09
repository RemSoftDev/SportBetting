using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;


namespace JHLib
{
    /// <summary>
    /// This class is simply for hanging some WPF-specific extension methods upon.
    /// </summary>
    public static class WPFExtensions
    {
        #region AlignToParent
        /// <summary>
        /// Causes the given Window to align itself alongside the parent (ie, Owner) window.
        /// You should put the call to this within the LayoutUpdated or Loaded event handler of the Window you want to align itself
        /// to it's parent. The Owner property has to have been set, otherwise there's nothing for it to align against.
        /// </summary>
        /// <param name="dialog">The given Window that is to be aligned</param>
        /// <param name="bRightSide">If true (Default), gives a preference toward aligning to the right (if there's room); otherwise tries to align to the left first</param>
        public static void AlignToParent(this Window dialog, bool bRightSide = true)
        {
            Window parent = dialog.Owner as Window;
            if (parent != null)
            {
                if (bRightSide)
                {
                    // Try the right side, then the left.
                    if (AlignToRight(dialog))
                    {
                        return;
                    }
                    else if (AlignToLeft(dialog))
                    {
                        return;
                    }
                }
                else
                {
                    // Try the left side, then the right.
                    if (AlignToLeft(dialog))
                    {
                        return;
                    }
                    else if (AlignToRight(dialog))
                    {
                        return;
                    }
                }
                // failing that, I'll try underneath
                // CBL  Is VirtualScreenTop always zero? !!!
                double VirtualScreenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
                double ParentBottom = parent.Top + parent.Height;
                double Separation = 2;

                if (ParentBottom < (VirtualScreenBottom - dialog.Height))
                {
                    dialog.Top = ParentBottom + Separation;
                    dialog.Left = Math.Max(parent.Left, SystemParameters.VirtualScreenLeft);
                }
                // failing that, I'll try over-top
                else if (parent.Top > dialog.Height)
                {
                    dialog.Top = parent.Top - dialog.Height - Separation;
                    dialog.Left = Math.Max(parent.Top, SystemParameters.VirtualScreenLeft);
                }
                // otherwise.. at this point I think it's time to throw in the towel and forget about it.
            }
        }

        #region internal helper methods for AlignToParent

        /// <summary>
        /// Just a helper method for AlignToParent
        /// </summary>
        /// <param name="dialog">the Window to align</param>
        /// <returns>true if successful, false if there wasn't sufficient space on the display</returns>
        private static bool AlignToRight(Window dialog)
        {
            Window parent = dialog.Owner as Window;
            double ParentRight = parent.Left + parent.Width;
            double Separation = 2;
            double ScreenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
            double VirtualScreenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

            bool bMultipleScreensHorz = false;
            if (SystemParameters.VirtualScreenWidth > SystemParameters.PrimaryScreenWidth)
            {
                bMultipleScreensHorz = true;
            }

            // This following, which brings in the need for Windows.Forms,
            // is for the case where a 2nd monitor is not as wide as the pri monitor.
            if (!bMultipleScreensHorz)
            {
                System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;
                if (allScreens.Count() > 1)
                {
                    System.Windows.Forms.Screen s2 = allScreens[1];
                    double width2 = s2.Bounds.Width;
                    double top2 = s2.Bounds.Top;
                    double bottom2 = s2.Bounds.Bottom;

                    if (parent.Top.IsInRange(top2, bottom2))
                    {
                        // Evidently, this Window is on Screen 2, so use it's area.
                        ScreenRight = width2;
                    }
                }
            }

            // I'll position myself along the right side if there's sufficient space.
            if (ParentRight + dialog.Width < ScreenRight)
            {
                // Try to avoid falling over top of the break between two screens.
                if (bMultipleScreensHorz)
                {
                    // See if the parent window is entirely within the leftmost screen.
                    if (ParentRight < SystemParameters.PrimaryScreenWidth)
                    {
                        if (ParentRight + dialog.Width > SystemParameters.PrimaryScreenWidth)
                        {
                            dialog.Left = SystemParameters.PrimaryScreenWidth;
                            dialog.Top = Math.Min(parent.Top, VirtualScreenBottom - dialog.Height);
                            return true;
                        }
                    }
                }
                dialog.Left = ParentRight + Separation;
                // Adjust my vertical position so that my feet don't jut off the bottom edge.
                dialog.Top = Math.Min(parent.Top, VirtualScreenBottom - dialog.Height);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Just a helper method for AlignToParent
        /// </summary>
        /// <param name="dialog">the Window to align</param>
        /// <returns>true if successful, false if there wasn't sufficient space on the display</returns>
        private static bool AlignToLeft(Window dialog)
        {
            Window parent = dialog.Owner as Window;
            double Separation = 2;
            double VirtualScreenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

            // Try the left side.
            if (parent.Left > (SystemParameters.VirtualScreenLeft + dialog.Width))
            {
                dialog.Left = parent.Left - dialog.Width - Separation;
                dialog.Top = Math.Min(parent.Top, VirtualScreenBottom - dialog.Height);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #endregion

        #region TextBox extension method InsertText
        /// <summary>
        /// Insert the given text into this TextBox at the current CaretIndex, and replacing any already-selected text.
        /// </summary>
        /// <param name="textbox">The TextBox to insert the new text into</param>
        /// <param name="sTextToInsert">The text to insert into this TextBox</param>
        public static void InsertText(this System.Windows.Controls.TextBox textbox, string sTextToInsert)
        {
            int iCaretIndex = textbox.CaretIndex;
            int iOriginalSelectionLength = textbox.SelectionLength;
            string sOriginalContent = textbox.Text;
            textbox.SelectedText = sTextToInsert;
            if (iOriginalSelectionLength > 0)
            {
                textbox.SelectionLength = 0;
            }
            textbox.CaretIndex = iCaretIndex + 1;
        }
        #endregion

        #region RichTextBox extension method InsertText
        /// <summary>
        /// Insert the given text into this RichTextBox at the current CaretPosition, and replacing any already-selected text.
        /// </summary>
        /// <param name="richTextBox">The RichTextBox to insert the new text into</param>
        /// <param name="sTextToInsert">The text to insert into this RichTextBox</param>
        public static void InsertText(this System.Windows.Controls.RichTextBox richTextBox, string sTextToInsert)
        {
            if (!String.IsNullOrEmpty(sTextToInsert))
            {
                richTextBox.BeginChange();
                if (richTextBox.Selection.Text != string.Empty)
                {
                    richTextBox.Selection.Text = string.Empty;
                }
                TextPointer tp = richTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
                richTextBox.CaretPosition.InsertTextInRun(sTextToInsert);
                richTextBox.CaretPosition = tp;
                richTextBox.EndChange();
                Keyboard.Focus(richTextBox);
            }
        }
        #endregion

        #region TextBox extension method TextTrimmed
        /// <summary>
        /// This is just a convenience extension-method to simplify the getting of strings
        /// from a WPF TextBox.
        /// It was a pain in da butt, having to remember to test for nulls, whitespace, etc.
        /// Now, all you have to do is check the .Length
        /// </summary>
        /// <param name="textbox">The WPF TextBox to get the Text from</param>
        /// <returns>If the TextBox was empty, then "" (empty string) otherwise the Text with leading and trailing whitespace trimmed</returns>
        public static string TextTrimmed(this System.Windows.Controls.TextBox textbox)
        {
            string sText = textbox.Text;
            if (String.IsNullOrEmpty(sText))
            {
                return "";
            }
            else
            {
                return sText.Trim();
            }
        }
        #endregion

        #region Window extension method MoveAsAGroup

        public static void MoveAsAGroup(this Window me,
                                        double desiredXDisplacement, double desiredYDisplacement,
                                        ref bool isIgnoringLocationChangedEvent)
        {
            // Ensure we don't recurse when we reposition.
            isIgnoringLocationChangedEvent = true;

            Window windowToMoveWith = me.Owner;

            // Try to prevent me from sliding off the screen horizontally.
            double bitToShow = 32;
            double leftLimit = SystemParameters.VirtualScreenLeft - me.Width + bitToShow;
            double rightLimit = SystemParameters.VirtualScreenWidth - bitToShow;
            bool notTooMuchXDisplacement = Math.Abs(me.Left - windowToMoveWith.Left) < Math.Abs(desiredXDisplacement);
            if (me.Left >= rightLimit && notTooMuchXDisplacement)
            {
                // bumping against the right.
                me.Left = rightLimit;
            }
            else if (me.Left <= leftLimit && notTooMuchXDisplacement)
            {
                // bumping against the left.
                me.Left = leftLimit;
            }
            else // it's cool - just slide along with the other window.
            {
                me.Left = windowToMoveWith.Left + desiredXDisplacement;
            }

            // Try to prevent me from sliding off the screen vertically.
            double topLimit = SystemParameters.VirtualScreenTop - me.Height + bitToShow;
            double bottomLimit = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - bitToShow;
            bool notTooMuchYDisplacement = Math.Abs(me.Top - windowToMoveWith.Top) < Math.Abs(desiredYDisplacement);
            if (me.Top <= topLimit && notTooMuchYDisplacement)
            {
                // bumping up against the top.
                //Console.WriteLine("setting to topLimit of " + topLimit);
                me.Top = topLimit;
            }
            else if (me.Top >= bottomLimit && notTooMuchYDisplacement)
            {
                // bumping against the bottom.
                me.Top = bottomLimit;
            }
            else // it's cool - just slide along with the other window.
            {
                me.Top = windowToMoveWith.Top + desiredYDisplacement;
            }

            // Reset the handler for the LocationChanged event.
            isIgnoringLocationChangedEvent = false;
        }
        #endregion

    }
}
