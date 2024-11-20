using MicaForEveryone.Win32.PInvoke;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MicaForEveryone.Win32
{
    public class AccentColor
    {


        private const int REG_NOTIFY_CHANGE_NAME = 0x1;
        private const int REG_NOTIFY_CHANGE_ATTRIBUTES = 0x2;
        private const int REG_NOTIFY_CHANGE_LAST_SET = 0x4;
        private const int REG_NOTIFY_CHANGE_SECURITY = 0x8;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, uint dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

        private static COLORREF accentColor = new COLORREF(0x000000);

        public static COLORREF GetAccentColor()
        {
            return accentColor;
        }

        private static RegistryKey accentKey;
        public static EventHandler AccentColorChanged;

        private static bool isInitialized = false;

        public static void InitializeListeners()
        {
            if(isInitialized)
            {
                return;
            }

            // Get accent color
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            // Event
            EmptyEventCallback callback = new EmptyEventCallback(AccentChangedEventCall);

            accentKey = key;

            Task.Run(AccentChangedEventCall);

            isInitialized = true;
        }

        delegate void EmptyEventCallback();

        static void AccentChangedEventCall()
        {
            while (true)
            {
                object color = accentKey.GetValue("AccentColor");
                if (color != null)
                {
                    if (color.GetType() == typeof(Int32))
                    {
                        UInt32 uColor = (UInt32)(Int32)color;

                        byte[] bytes = BitConverter.GetBytes(uColor);

                        byte r = bytes[0];
                        byte g = bytes[1];
                        byte b = bytes[2];

                        accentColor = new COLORREF(r, g, b);
                    }
                }

                Console.WriteLine($"COLOR: {color}");


                int hEvent = RegNotifyChangeKeyValue(accentKey.Handle.DangerousGetHandle(), true,
                    REG_NOTIFY_CHANGE_NAME | REG_NOTIFY_CHANGE_ATTRIBUTES | REG_NOTIFY_CHANGE_LAST_SET | REG_NOTIFY_CHANGE_SECURITY,
                    IntPtr.Zero, false);
                AccentColorChanged.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
