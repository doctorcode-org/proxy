using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DoctorProxy.Control
{
    public class LinkButton : Button
    {
        public bool Selected
        {
            get { return (bool)this.GetValue(SelectedProperty); }
            set { this.SetValue(SelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached("Selected", typeof(bool), typeof(LinkButton), new PropertyMetadata(false));

        [Description("The text displayed by the button."), Category("Extended Properties")]
        public string Caption
        {
            get { return (string)this.GetValue(CaptionProperty); }
            set { this.SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached("Caption", typeof(string), typeof(LinkButton), new PropertyMetadata(null));


        [Description("The image displayed by the button."), Category("Extended Properties")]
        public BitmapSource Icon
        {
            get { return (BitmapSource)this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(BitmapSource), typeof(LinkButton), new PropertyMetadata(null));


        [Description("The image displayed by the button on mouseover."), Category("Extended Properties")]
        public BitmapSource HoverIcon
        {
            get { return (BitmapSource)this.GetValue(HoverIconProperty); }
            set { this.SetValue(HoverIconProperty, value); }
        }

        public static readonly DependencyProperty HoverIconProperty = DependencyProperty.RegisterAttached("HoverIcon", typeof(BitmapSource), typeof(LinkButton), new PropertyMetadata(null));
    }
}
