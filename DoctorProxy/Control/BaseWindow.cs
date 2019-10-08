using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DoctorProxy.Control
{
    public class BaseWindow : Window
    {
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public static readonly DependencyProperty CaptionBackgroundProperty;
        public static readonly DependencyProperty CaptionHeightProperty;
        public static readonly DependencyProperty RenderIconProperty;

        
        public BaseWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseCommand));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, OnShowSystemMenuCommand));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindowCommand));

            this.Resources.MergedDictionaries.Add(App.CultureResource);
        }

        static BaseWindow()
        {
            CaptionBackgroundProperty = DependencyProperty.RegisterAttached("CaptionBackground", typeof(Brush), typeof(BaseWindow), new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#6389a8"))));
            CaptionHeightProperty = DependencyProperty.RegisterAttached("CaptionHeight", typeof(int), typeof(BaseWindow), new PropertyMetadata(35));
            RenderIconProperty = DependencyProperty.RegisterAttached("RenderIcon", typeof(Visibility), typeof(BaseWindow), new PropertyMetadata(Visibility.Visible));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow), new FrameworkPropertyMetadata(typeof(BaseWindow)));
        }

        private void OnMinimizeWindowCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void OnCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            int WM_CLOSE = 0x0010;
            SendMessage(new WindowInteropHelper(this).Handle, WM_CLOSE, 0, 0);
        }

        private void OnShowSystemMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var x = (int)this.Left;
            var y = (int)this.Top + 35;
            int point = ((y << 16) | (x & 0xffff));
            var hwnd = new WindowInteropHelper(this).Handle;
            SendMessage(hwnd, 0x313, 0, point);
        }




        [Bindable(true)]
        [Category("Appearance")]
        public Brush CaptionBackground
        {
            get { return (Brush)this.GetValue(CaptionBackgroundProperty); }
            set { this.SetValue(CaptionBackgroundProperty, value); }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public int CaptionHeight
        {
            get { return (int)this.GetValue(CaptionHeightProperty); }
            set { this.SetValue(CaptionHeightProperty, value); }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public Visibility RenderIcon
        {
            get { return (Visibility)this.GetValue(RenderIconProperty); }
            set { this.SetValue(RenderIconProperty, value); }
        }

    }
}
