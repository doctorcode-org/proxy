using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Text.RegularExpressions;

namespace DoctorProxy.Control
{
    public class ServiceSelector : ContentControl
    {
        public event TextCompositionEventHandler TextboxPreviewTextInput;

        public static readonly DependencyProperty CheckedProperty;
        public static readonly DependencyProperty LabelTextProperty;
        public static readonly DependencyProperty LabelWidthProperty;
        public static readonly DependencyProperty MaxLengthProperty;
        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty NumericTextBoxProperty;
        public static readonly DependencyProperty LabelDirectionProperty;
        static ServiceSelector()
        {
            CheckedProperty = DependencyProperty.RegisterAttached("Checked", typeof(bool), typeof(ServiceSelector), new PropertyMetadata(false));
            LabelTextProperty = DependencyProperty.RegisterAttached("LabelText", typeof(string), typeof(ServiceSelector), new PropertyMetadata(null));
            LabelWidthProperty = DependencyProperty.RegisterAttached("LabelWidth", typeof(GridLength), typeof(ServiceSelector), new PropertyMetadata(GridLength.Auto));
            MaxLengthProperty = DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(ServiceSelector), new PropertyMetadata(5));
            ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(string), typeof(ServiceSelector), new PropertyMetadata(null));
            NumericTextBoxProperty = DependencyProperty.RegisterAttached("NumericTextBox", typeof(bool), typeof(ServiceSelector), new PropertyMetadata(true));
            LabelDirectionProperty = DependencyProperty.RegisterAttached("LabelDirection", typeof(FlowDirection), typeof(ServiceSelector), new PropertyMetadata(FlowDirection.LeftToRight));
        }

        public bool Checked
        {
            get { return (bool)this.GetValue(CheckedProperty); }
            set { this.SetValue(CheckedProperty, value); }
        }


        public string LabelText
        {
            get { return (string)this.GetValue(LabelTextProperty); }
            set { this.SetValue(LabelTextProperty, value); }
        }


        public GridLength LabelWidth
        {
            get { return (GridLength)this.GetValue(LabelWidthProperty); }
            set { this.SetValue(LabelWidthProperty, value); }
        }

        public FlowDirection LabelDirection
        {
            get { return (FlowDirection)this.GetValue(LabelDirectionProperty); }
            set { this.SetValue(LabelDirectionProperty, value); }
        }

        public int MaxLength
        {
            get { return (int)this.GetValue(MaxLengthProperty); }
            set { this.SetValue(MaxLengthProperty, value); }
        }

        public bool NumericTextBox
        {
            get { return (bool)this.GetValue(NumericTextBoxProperty); }
            set { this.SetValue(NumericTextBoxProperty, value); }
        }

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.NumericTextBox)
            {
                var textbox = FindVisualChildren<TextBox>(this).FirstOrDefault();
                if (textbox != null)
                {
                    textbox.PreviewTextInput += textbox_PreviewTextInput;
                    textbox.PreviewKeyDown += textbox_PreviewKeyDown;
                    textbox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_CommandExecuted));
                }
            }
            else if (TextboxPreviewTextInput != null)
            {
                var textbox = FindVisualChildren<TextBox>(this).FirstOrDefault();
                if (textbox != null)
                    textbox.PreviewTextInput += TextboxPreviewTextInput;
            }
        }

        void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void Paste_CommandExecuted(object sender, RoutedEventArgs e)
        {
            e.Handled = false;
            //if ((e as ExecutedRoutedEventArgs).Command == ApplicationCommands.Paste)
            //{
            //    if (Clipboard.ContainsText())
            //    {
            //        var text = Clipboard.GetText();
            //        if (IsTextAllowed(text) == false)
            //            e.Handled = true;
            //    }
            //}
        }

        private List<T> FindVisualChildren<T>(DependencyObject depObj, bool searchChild = true) where T : DependencyObject
        {
            var result = new List<T>();

            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        result.Add((T)child);
                    }

                    if (searchChild)
                    {
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            result.Add(childOfChild);
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex(@"^\d$"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
    }
}
