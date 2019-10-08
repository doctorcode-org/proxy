using System.Security;
using System.Windows;

namespace DoctorProxy
{
    internal class MsgBox
    {
        internal static MessageBoxResult result;

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(messageBoxText, "");
        }

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(messageBoxText, caption, MessageBoxButton.OK);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(messageBoxText, caption, button, MessageBoxImage.None);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return Show(null, messageBoxText, caption, button, icon, defaultResult, options);
        }

        /*_______________________________________________________________________________________________________________________________________________________________*/

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            return Show(owner, messageBoxText, "");
        }

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            return Show(owner, messageBoxText, caption, MessageBoxButton.OK);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(owner, messageBoxText, caption, button, MessageBoxImage.None);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(owner, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        [SecurityCritical]
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            var win = new MsgBoxWindow();
            win.Title = caption;
            win.Owner = owner;
            win.ShowDialog();
            return result;
        }

    }


}
