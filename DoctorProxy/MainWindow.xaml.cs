using DoctorProxy.Control;
using DoctorProxy.EventLoger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DoctorProxy
{
    public partial class MainWindow : BaseWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            SettingsView.DataContext = App.DbContext.GetSettings();
            grpServiceManager.DataContext = App.ProxyService;
        }



        private void MainLinkButton_Click(object sender, RoutedEventArgs e)
        {
            var allLinks = FindVisualChildren<LinkButton>(RightPanel);

            allLinks.ForEach(link =>
            {
                link.Selected = false;
            });

            var button = sender as LinkButton;
            button.Selected = true;
        }

        private T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private List<T> FindVisualChildren<T>(DependencyObject depObj, bool searchChild = false) where T : DependencyObject
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

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var data = (DoctorProxy.DataModel.Settings)SettingsView.DataContext;
            var result = App.DbContext.SaveSettings(data);
            if (result == true)
                MsgBox.Show(this, "ذخیره تغییرات با موفقیت انجام شد.", "ذخیره تغییرات");
            else
                MsgBox.Show(this, "خطایی در ذخیره تغییرات رخ داده است، برای جزئیات بیشتر رخدادهای نرم‌افزار را بررسی کنید.", "خطای ذخیره تغییرات");
        }


        /*______________________________________________________________________________________________________________________________________________*/

        private async void btnInstallService_Click(object sender, RoutedEventArgs e)
        {
            LoaderWindow.Show(this, "نصب سرویس ...");


            var svcManager = new Service.Manager();
            var path = String.Format("{0}\\Service.exe", Environment.CurrentDirectory);
            var result = svcManager.InstallService(path, App.ProxyService.ServiceName, "Doctor Proxy Service");
            App.ProxyService.Installed = result;

            await Task.Delay(1000);

            if (result == true)
                App.ProxyService.IsRunning = await RunService();

            LoaderWindow.CloseLoader();

            if (result != true)
                MessageBox.Show("نصب سرویس امکان‌پذیر نیست، برای بررسی خطای رخ داده، رخدادهای نرم‌افزار را بررسی کنید.", "خطای نصب سرویس");
        }

        private async void btnUnistallService_Click(object sender, RoutedEventArgs e)
        {
            LoaderWindow.Show(this, "حذف سرویس ...");
            var result = false;

            result = await Task.Run(() =>
            {
                var svcManager = new Service.Manager();
                svcManager.StopService(App.ProxyService.ServiceName, 10000);
                return svcManager.UninstallService(App.ProxyService.ServiceName);
            });

            LoaderWindow.CloseLoader();

            if (result == true)
            {
                App.ProxyService.Installed = false;
                App.ProxyService.IsRunning = false;
            }
            else
            {
                MessageBox.Show("حذف سرویس امکان‌پذیر نیست، برای بررسی خطای رخ داده، رخدادهای نرم‌افزار را بررسی کنید.", "خطای حذف سرویس");
            }
        }

        private async void btnRunService_Click(object sender, RoutedEventArgs e)
        {
            LoaderWindow.Show(this, "اجرای سرویس ...");

            var result = await RunService();
            App.ProxyService.IsRunning = result;
            LoaderWindow.CloseLoader();

            if (result == false)
                MessageBox.Show("اجرای سرویس امکان‌پذیر نیست، برای بررسی خطای رخ داده، رخدادهای نرم‌افزار را بررسی کنید.", "خطای اجرای سرویس");
        }

        private async void btnStopService_Click(object sender, RoutedEventArgs e)
        {
            LoaderWindow.Show(this, "توقف سرویس ...");

            var result = await Task.Run(() =>
            {
                var svcManager = new Service.Manager();
                return svcManager.StopService(App.ProxyService.ServiceName, 10000);
            });

            App.ProxyService.IsRunning = !result;

            LoaderWindow.CloseLoader();

            if (result == false)
                MessageBox.Show("توقف سرویس امکان‌پذیر نیست، برای بررسی خطای رخ داده، رخدادهای نرم‌افزار را بررسی کنید.", "خطای توقف سرویس");
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            var win = new WinNewUser();
            win.Owner = this;
            win.ShowDialog();
        }

        private void btnMakeCert_Click(object sender, RoutedEventArgs e)
        {
            var win = new WinCertMaker();
            win.Owner = this;
            win.ShowDialog();
        }

        private async Task<bool> RunService()
        {
            if (await CheckDHFile())
            {
                var svcManager = new Service.Manager();
                return svcManager.StartService(App.ProxyService.ServiceName, 10000);
            }

            return false;
        }

        private async Task<bool> CheckDHFile()
        {
            var result = true;
            var basePath = Environment.CurrentDirectory;
            var keysFolder = String.Format("{0}\\keys", basePath);
            var dhPath = String.Format("{0}\\dh.pem", keysFolder);
            var openSslPath = String.Format("{0}\\openssl.exe", basePath);
            var configPath = String.Format("{0}\\config.cnf", basePath);

            try
            {
                if (!Directory.Exists(keysFolder))
                    Directory.CreateDirectory(keysFolder);

                if (!File.Exists(dhPath))
                {
                    var args = string.Format("dhparam -out {0} 2048", dhPath);
                    var manager = new ProcessManager(300000)
                    {
                        OnError = (ex) =>
                        {
                            result = false;
                            Log.Write(MethodInfo.GetCurrentMethod(), ex);
                        }
                    };

                    result = (await manager.Start(openSslPath, args) && File.Exists(dhPath));
                }
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }
            
            return result;
        }

        /*______________________________________________________________________________________________________________________________________________*/

    }
}
