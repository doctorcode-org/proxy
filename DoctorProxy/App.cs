using DoctorProxy.Data;
using DoctorProxy.ViewModels;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Linq;

namespace DoctorProxy
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            cultureResource = new ResourceDictionary();
            cultureResource.Source = GetCultureUri();
        }



        private static DbContext dbContext = new DbContext();
        public static DbContext DbContext
        {
            get { return dbContext; }
        }

        private static ServiceModel proxyService = new ServiceModel();
        public static ServiceModel ProxyService
        {
            get { return proxyService; }
        }

        private static ResourceDictionary cultureResource;
        public static ResourceDictionary CultureResource
        {
            get
            {
                return cultureResource;
            }
        }


        /*___________________________________________________________________________________________________________________________________________*/
        
        public static string CultureString(string key)
        {
            //if (cultureResource.Contains(key))
            //    return cultureResource[key].ToString();
            //else if (defaultCultureResource.Contains(key))
            //    return defaultCultureResource[key].ToString();

            return "";
        }

        private static string[] GetResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToArray();
                }
            }
        }

        private static Uri GetCultureUri()
        {
            var calture = Thread.CurrentThread.CurrentCulture;
            var name = calture.ToString();

            var resources = GetResourceNames();

            if (resources.Any(key => key == String.Format("resources/stringresources.{0}.baml", name.ToLower())))
            {
                return new Uri(String.Format("..\\Resources\\StringResources.{0}.xaml", name), UriKind.Relative);
            }
            else if (resources.Any(key => key == String.Format("resources/stringresources.{0}.baml", calture.Parent)))
            {
                return new Uri(String.Format("..\\Resources\\StringResources.{0}.xaml", calture.Parent), UriKind.Relative);
            }

            return new Uri("..\\Resources\\StringResources.xaml", UriKind.Relative);
        }


    }


}
