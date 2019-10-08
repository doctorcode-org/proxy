using DoctorProxy.Control;
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
    /// <summary>
    /// Interaction logic for MsgBoxWindow.xaml
    /// </summary>
    public partial class MsgBoxWindow : BaseWindow
    {
        public MsgBoxWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.RenderIcon = System.Windows.Visibility.Hidden;
        }






        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MsgBox.result = MessageBoxResult.OK;
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            MsgBox.result = MessageBoxResult.Yes;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            MsgBox.result = MessageBoxResult.No;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MsgBox.result = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
