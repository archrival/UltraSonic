using System;
using System.Drawing.Printing;
using System.Security;
using System.Windows;
using System.Windows.Interop;

namespace UltraSonic.Static
{
    public static class Dwm
    {
        /// <summary> 
        /// Drops a standard shadow to a WPF Window, even if the window isborderless. Only works with DWM (Vista and Seven). 
        /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect, 
        /// as AllowsTransparency involves a huge performance issue (hardware acceleration is turned off for all the window). 
        /// </summary> 
        /// <param name="window">Window to which the shadow will be applied</param> 
        public static void DropShadowToWindow(Window window)
        {
            if (!DropShadow(window))
                window.SourceInitialized += WindowSourceInitialized;
        }

        private static void WindowSourceInitialized(object sender, EventArgs e)
        {
            Window window = (Window)sender;

            DropShadow(window);

            window.SourceInitialized -= WindowSourceInitialized;
        }

        /// <summary> 
        /// The actual method that makes API calls to drop the shadow to the window 
        /// </summary> 
        /// <param name="window">Window to which the shadow will be applied</param> 
        /// <returns>True if the method succeeded, false if not</returns> 
       [SecurityCritical]
        private static bool DropShadow(Window window)
        {
            try
            {
                if (!NativeMethods.DwmIsCompositionEnabled())
                    return false;

                WindowInteropHelper helper = new WindowInteropHelper(window);
                int val = 2;
                int ret1 = NativeMethods.DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 != 0)
                    return false;

                Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
                int ret2 = NativeMethods.DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                return ret2 == 0;
            }
            catch (Exception)
            {
                // Probably dwmapi.dll not found (incompatible OS) 
                return false;
            }
        }
    }
}
