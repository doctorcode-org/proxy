using DoctorProxy.Control;
using DoctorProxy.DataModel;
using DoctorProxy.EventLoger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public partial class WinCertMaker : BaseWindow
    {
        private CA certModel;

        private const string caFormat = "openssl req -days {0} -nodes -newkey rsa:{1} -new -x509 -keyout ca.key -out ca.crt -config config.cnf -subj \"{subject}\"";
        private const string subjectFormat = "/C={0}/ST={1}/L={2}/O={3}/CN={4}/OU={5}";

        private string basePath;
        private string keysFolder;

        private string caPath;
        private string keyPath;

        private string serverCaPath;
        private string serverKeyPath;
        private string serverCsrPath;

        private string clientCaPath;
        private string clientKeyPath;
        private string clientCsrPath;

        private string openSslPath;
        private string configPath;

        private string dbPath;
        private string serialPath;


        public WinCertMaker()
        {
            InitializeComponent();

            certModel = new CA();

            spNewCa.DataContext = certModel;

            basePath = Environment.CurrentDirectory;
            keysFolder = String.Format("{0}\\keys", basePath);

            caPath = String.Format("{0}\\ca.crt", keysFolder);
            keyPath = String.Format("{0}\\ca.key", keysFolder);

            serverCaPath = String.Format("{0}\\server.crt", keysFolder);
            serverKeyPath = String.Format("{0}\\server.key", keysFolder);
            serverCsrPath = String.Format("{0}\\server.csr", keysFolder);

            clientCaPath = String.Format("{0}\\client.crt", keysFolder);
            clientKeyPath = String.Format("{0}\\client.key", keysFolder);
            clientCsrPath = String.Format("{0}\\client.csr", keysFolder);

            openSslPath = String.Format("{0}\\openssl.exe", basePath);
            configPath = String.Format("{0}\\config.cnf", basePath);

            dbPath = String.Format("{0}\\index", keysFolder);
            serialPath = String.Format("{0}\\serial", keysFolder);
        }


        private async void btnMakeCert_Click(object sender, RoutedEventArgs e)
        {
            var success = false;

            if (String.IsNullOrEmpty(certModel.CommonName))
            {
                MessageBox.Show("CN را وارد کنید.", "خطای پردازش");
                return;
            }

            LoaderWindow.Show(this, "صدور گواهی ...");

            var svcManager = new Service.Manager();
            if (svcManager.StopService(App.ProxyService.ServiceName, 10000))
            {
                App.ProxyService.IsRunning = false;

                try
                {
                    if (Directory.Exists(keysFolder))
                        FileExtentions.DeleteFilesExept(keysFolder, "dh.pem");
                    else
                        Directory.CreateDirectory(keysFolder);

                    using (var fsDb = File.Create(dbPath)) { };
                    using (var fsSr = File.Create(serialPath))
                    {
                        var data = Encoding.Default.GetBytes("01\n");
                        fsSr.Write(data, 0, data.Length);
                        fsSr.Close();
                    };

                    if (await MakeIssuerCA() && await MakeCsr(true) && await MakeCsr(false))
                    {
                        if (await MakeCsr(true))
                        {
                            success = true;
                            FileExtentions.DeleteFilesExept(keysFolder, "dh.pem", "ca.crt", "server.crt", "server.key", "client.crt", "client.key");
                            MessageBox.Show("صدور گواهی با موفقیت انجام شد.\n یادآوری: تنظیمات کلاینت را دوباره دریافت کنید.", "صدور گواهی");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(MethodInfo.GetCurrentMethod(), ex);
                    if (!success)
                        MessageBox.Show("در عملیات صدور گواهی خطایی رخ داده است، برای آگاهی بیشتر رخدادهای نرم‌افزار را بررسی کنید.", "خطای صدور گواهی");
                }

                LoaderWindow.UpdateStatus("اجرای سرویس ...");
                App.ProxyService.IsRunning = svcManager.StartService(App.ProxyService.ServiceName, 10000);
                LoaderWindow.CloseLoader();

                if (success)
                    this.Close();
            }
        }

        /// <summary>
        /// تولید گواهی امضا کننده
        /// </summary>
        /// <returns></returns>
        private async Task<bool> MakeIssuerCA()
        {
            var result = false;
            try
            {
                var co = certModel.Country.ConvertNullOrEmptyTo("IR");
                var st = certModel.State.ConvertNullOrEmptyTo("ST");
                var loc = certModel.Locale.ConvertNullOrEmptyTo("Local");
                var org = certModel.Organization.ConvertNullOrEmptyTo("DoctorCode");
                var ou = certModel.OrganizationUnit.ConvertNullOrEmptyTo("DoctorProxy");

                var subject = String.Format(subjectFormat, co, st, loc, org, "www.doctorproxy.net", ou);
                var args = String.Format("req -days {0} -nodes -newkey rsa:{1} -new -x509 -keyout {2} -out {3} -config {4} -subj \"{5}\"", certModel.ExpireDays, certModel.KeyLenght, keyPath, caPath, configPath, subject);

                var manager = new ProcessManager(15000)
                {
                    OnError = (ex) =>
                    {
                        result = false;
                        Log.Write(MethodInfo.GetCurrentMethod(), ex);
                    }
                };

                result = await manager.Start(openSslPath, args);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            return result;
        }

        /// <summary>
        /// تولید گواهی امضا شده
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private async Task<bool> SignCertificate(bool server)
        {
            var result = false;

            try
            {
                var ca2Path = server ? serverCaPath : clientCaPath;
                var csrPath = server ? serverCsrPath : clientCsrPath;
                var extenst = server ? " -extensions server" : "";

                var args = String.Format("ca -days {0} -batch -out {1} -keyfile {2} -cert {3} -md sha1 -in {4} -config {5} -outdir {6}{7}",
                    certModel.ExpireDays, ca2Path, keyPath, caPath, csrPath, configPath, keysFolder, extenst);

                var manager = new ProcessManager(30000)
                {
                    OnError = (ex) =>
                    {
                        result = false;
                        Log.Write(MethodInfo.GetCurrentMethod(), ex);
                    }
                };

                result = await manager.Start(openSslPath, args);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }

            return result;
        }

        /// <summary>
        /// تولید گواهی درخواست
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private async Task<bool> MakeCsr(bool server)
        {
            var result = false;

            try
            {
                var co = certModel.Country.ConvertNullOrEmptyTo("IR");
                var st = certModel.State.ConvertNullOrEmptyTo("ST");
                var loc = certModel.Locale.ConvertNullOrEmptyTo("Local");
                var org = certModel.Organization.ConvertNullOrEmptyTo("DoctorCode");
                var ou = certModel.OrganizationUnit.ConvertNullOrEmptyTo("DoctorProxy");

                var subject = String.Format(subjectFormat, co, st, loc, org, certModel.CommonName, ou);
                var key2Path = server ? serverKeyPath : clientKeyPath;
                var csrPath = server ? serverCsrPath : clientCsrPath;

                var args = String.Format("req -days {0} -nodes -newkey rsa:{1} -new -keyout {2} -out {3} -config {4} -subj {5}", certModel.ExpireDays, certModel.KeyLenght, key2Path, csrPath, configPath, subject);


                var manager = new ProcessManager(60000)
                {
                    OnError = (ex) =>
                    {
                        result = false;
                        Log.Write(MethodInfo.GetCurrentMethod(), ex);
                    }
                };

                result = await manager.Start(openSslPath, args);

                if (result)
                    result = await SignCertificate(server);
            }
            catch (Exception ex)
            {
                Log.Write(MethodInfo.GetCurrentMethod(), ex);
            }


            return result;
        }

    }


    public static class FileExtentions
    {
        public static void DeleteFilesExept(string path, params string[] exeptFiles)
        {
            try
            {
                var files = Directory.GetFiles(path).Where(f => exeptFiles.Contains(new FileInfo(f).Name) == false).ToList();
                files.ForEach((file) => { File.Delete(file); });
            }
            catch { }
        }

        public static void DeleteFiles(string path, string patern)
        {
            var files = Directory.GetFiles(path, patern);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }

            }
        }

        public static void DeleteFiles(params string[] filesToDelete)
        {
            foreach (var file in filesToDelete)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

    }

    public static class StringExtentions
    {
        public static string NullIfEmpty(this string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            return str;
        }

        public static string ConvertNullOrEmptyTo(this string str, string replace)
        {
            if (String.IsNullOrEmpty(str))
                return replace;
            return str;
        }
    }
}
