using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoctorProxy
{
    public partial class LoaderWindow : Window
    {
        private static LoaderWindow win;

        public static void Show(Window owner, string title)
        {
            win = new LoaderWindow();
            win.Owner = owner;
            win.Width = owner.ActualWidth;
            win.Height = owner.ActualHeight;
            win.Title = title;
            win.Show();
        }

        public static void UpdateStatus(string state)
        {
            win.Title = state;
        }

        public static void CloseLoader()
        {
            if (win != null)
                win.Close();
        }

        public LoaderWindow()
        {
            InitializeComponent();
        }

        public static void Increase(double value)
        {
            if (win != null)
            {
                var newValue = win.Loader.Value + value;
                win.Loader.Value = (newValue > 100) ? 100 : newValue;
            }
        }

        public static void Update(double value)
        {
            if (win != null)
            {
                if (value < 0)
                    value = 0;
                else if (value > 100)
                    value = 100;

                win.Loader.Value = value;
            }
        }
    }
}
