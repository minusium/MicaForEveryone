using MicaForEveryone.Win32.PInvoke;
using System;
using System.Runtime.InteropServices;
using static MicaForEveryone.Win32.PInvoke.NativeMethods;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace MicaForEveryone.Win32
{
    public static class DesktopWindowManager
    {
        /// <summary>
        /// Check whether Windows build is 19041 or higher, that supports <see cref="SetImmersiveDarkMode(IntPtr, bool)"/>.
        /// </summary>
        public static bool IsImmersiveDarkModeSupported { get; } =
            Environment.OSVersion.Version.Build >= 19041;

        /// <summary>
        /// Check whether Windows build is 22000 or higher, that supports <see cref="SetMica(IntPtr, bool)"/>.
        /// </summary>
        public static bool IsUndocumentedMicaSupported { get; } =
            Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Check whether Windows Windows build is 22523 or higher, that supports <see cref="SetBackdropType(IntPtr, DWM_SYSTEMBACKDROP_TYPE)"/>
        /// </summary>
        public static bool IsBackdropTypeSupported { get; } =
            Environment.OSVersion.Version.Build >= 22523;

        /// <summary>
        /// Check whether Windows Windows build is 22000 or higher, that supports <see cref="SetCornerPreference(IntPtr, DWM_WINDOW_CORNER_PREFERENCE)"/>
        /// </summary>
        public static bool IsCornerPreferenceSupported { get; } =
            Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Check whether Windows Windows build is 22000 or higher, that supports <see cref="SetCaptionColor(IntPtr, COLORREF)"/>
        /// </summary>
        public static bool IsCaptionColorSupported { get; } =
            Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Check whether Windows Windows build is 22000 or higher, that supports <see cref="SetCaptionTextColor(IntPtr, COLORREF)"/>
        /// </summary>
        public static bool IsCaptionTextColorSupported { get; } =
            Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Check whether Windows Windows build is 22000 or higher, that supports <see cref="SetBorderColor(IntPtr, COLORREF)"/>
        /// </summary>
        public static bool IsBorderColorSupported { get; } =
            Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Enable Mica on target window with <see cref="SetMica(IntPtr, bool)"/> or <see cref="SetBackdropType(IntPtr, DWM_SYSTEMBACKDROP_TYPE)"/> if supported.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        public static void EnableMicaIfSupported(IntPtr hWnd)
        {
#if NET6_0_OR_GREATER
#pragma warning disable CA1416
#endif
            if (IsBackdropTypeSupported)
            {
                SetBackdropType(hWnd, DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW);
            }
            else if (IsUndocumentedMicaSupported)
            {
                SetMica(hWnd, true);
            }
#if NET6_0_OR_GREATER
#pragma warning restore CA1416
#endif
        }

        /// <summary>
        /// Enable or Disable Mica on target window
        /// Supported on Windows builds from 22000 to 22523. It doesn't work on 22523, use <see cref="SetBackdropType(IntPtr, DWM_SYSTEMBACKDROP_TYPE)"/> instead.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="state"><see langword="true"/> to enable Mica effects on the target window, <see langword="false"/> to disable them.</param>
        /// <seealso cref="SetBackdropType(IntPtr, DWM_SYSTEMBACKDROP_TYPE)"/>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22000")]
        [UnsupportedOSPlatform("windows10.0.22523")]
# endif
        public static void SetMica(IntPtr hWnd, bool state)
        {
            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_MICA, state ? 1 : 0, sizeof(int));
        }

        /// <summary>
        /// Set backdrop type on target window
        /// Requires Windows build 22523 or higher.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="backdropType">Type of backdrop to apply to the target window's background.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22523")]
#endif
        public static void SetBackdropType(IntPtr hWnd, DWM_SYSTEMBACKDROP_TYPE backdropType)
        {
            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, (uint)backdropType, sizeof(uint));
        }

        /// <summary>
        /// Set corner preference on target window
        /// Requires Windows build 22000 or higher.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="cornerPreference">Corner preference to apply on the window's border.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22000")]
#endif
        public static void SetCornerPreference(IntPtr hWnd, DWM_WINDOW_CORNER_PREFERENCE cornerPreference)
        {
            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE, (uint)cornerPreference, sizeof(uint));
        }

        /// <summary>
        /// Enable or disable immersive dark mode.
        /// Requires Windows build 19041 or higher.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="state"><see langword="true"/> to enable the immersive dark mode, <see langword="false"/> to disable it.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.19041")]
#endif
        public static void SetImmersiveDarkMode(IntPtr hWnd, bool state)
        {
            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, state ? 1 : 0, sizeof(int));
        }

        /// <summary>
        /// Extends the window's frame into the client area.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        public static void ExtendFrameIntoClientArea(IntPtr hWnd)
        {
            var margins = new MARGINS(-1);
            CheckHResult(DwmExtendFrameIntoClientArea(hWnd, margins));
        }

        /// <summary>
        /// Change background color of titlebar.
        /// Requires Windows build 22000 or higher.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="color">Color value to apply to the window's caption.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22000")]
#endif
        public static void SetCaptionColor(IntPtr hWnd, COLORREF color)
        {
            if (Environment.OSVersion.Version.Build < 22000)
                return;
            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_CAPTION_COLOR, color, Marshal.SizeOf(color));
        }

        /// <summary>
        /// Change text color of titlebar.
        /// Requires Windows build 22000 or higher.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        /// <param name="color">Color value to apply to the window's caption text.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22000")]
#endif
        public static void SetCaptionTextColor(IntPtr hWnd, COLORREF color)
        {
            if (Environment.OSVersion.Version.Build < 22000)
                return;

            SetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_TEXT_COLOR, color, Marshal.SizeOf(color));
        }

        /// <summary>
        /// Change border color of window.
        /// Requires Windows build 22000 or higher.
        /// </summary>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows10.0.22000")]
#endif
        public static void SetBorderColor(IntPtr hWnd, COLORREF color)
        {
            if (Environment.OSVersion.Version.Build < 22000)
                return;

            var value = GCHandle.Alloc(color, GCHandleType.Pinned);
            var result = DwmSetWindowAttribute(hWnd, DWMWA_BORDER_COLOR, value.AddrOfPinnedObject(), Marshal.SizeOf(color));
            value.Free();
            if (result != 0)
            {
                throw Marshal.GetExceptionForHR(result);
            }
        }

        /// Enables the Blur effects to be rendered in the window's background.
        /// </summary>
        /// <param name="hWnd">Handle of the target window for the operation.</param>
        public static void EnableBlurBehind(IntPtr hWnd)
        {
            var value = new DWM_BLURBEHIND(true);
            CheckHResult(DwmEnableBlurBehindWindow(hWnd, value));

            var accentPolicy = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND | AccentState.ACCENT_ENABLE_GRADIENT,
                GradientColor = (152 << 24) | (0x2B2B2B & 0xFFFFFF),
            };
            var accentSize = Marshal.SizeOf(accentPolicy);
            var accentPolicyPtr = Marshal.AllocHGlobal(accentSize);
            Marshal.StructureToPtr(accentPolicy, accentPolicyPtr, false);
            var compositionAttributeData = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                Data = accentPolicyPtr,
            };
            compositionAttributeData.SizeOfData = Marshal.SizeOf(compositionAttributeData);
            var result = SetWindowCompositionAttribute(hWnd, ref compositionAttributeData);
            Marshal.FreeHGlobal(accentPolicyPtr);
            CheckHResult(result);
        }

        private static void SetWindowAttribute<T>(IntPtr hWnd, DwmWindowAttribute attribute, T value, int sizeOf)
        {
            var pinnedValue = GCHandle.Alloc(value, GCHandleType.Pinned);
            var valueAddr = pinnedValue.AddrOfPinnedObject();
            var result = DwmSetWindowAttribute(hWnd, (uint)attribute, valueAddr, sizeOf);
            pinnedValue.Free();
            CheckHResult(result);
        }

        private static int CheckHResult(int result)
        {
            if (Marshal.GetExceptionForHR(result) is { } ex) throw ex;
            return result;
        }
    }
}
