using System.Windows;
using System.Windows.Controls;

namespace DoctorProxy.Control
{
    public class SlideView : ContentControl
    {
        public bool Selected
        {
            get { return (bool)this.GetValue(SelectedProperty); }
            set { this.SetValue(SelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.RegisterAttached("Selected", typeof(bool), typeof(SlideView), new PropertyMetadata(false));

    }
}
