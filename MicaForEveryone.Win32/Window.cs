using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using MicaForEveryone.Win32.PInvoke;

#nullable enable

namespace MicaForEveryone.Win32
{
    public class Window : IDisposable
    {
        public const int CW_USEDEFAULT = unchecked((int)0x80000000);

        private const uint ERROR_MOD_NOT_FOUND = 0x0000007E;
        private const uint USER_DEFAULT_SCREEN_DPI = 96;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        public static void AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle, WindowStylesEx dwExStyle)
        {
            if (!NativeMethods.AdjustWindowRectEx(ref lpRect, dwStyle, false, dwExStyle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Get a Window object for given window handle.
        /// </summary>
        public static Window FromHandle(IntPtr hWnd)
        {
            var result = new Window
            {
                Handle = hWnd,
                Class = WindowClass.GetClassOfWindow(hWnd),
            };
            result.InstanceHandle = result.Class?.Module ?? IntPtr.Zero;
            return result;
        }

        /// <summary>
        /// Get a Window object if given window handle is a valid UI Automation window pattern.
        /// </summary>
        /// <returns>Null if handle is not valid, window object if valid.</returns>
        public static Window? GetWindowIfWindowPatternValid(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;

            if (!NativeMethods.IsWindow(hWnd))
                return null;

            if (!NativeMethods.IsWindowVisible(hWnd))
                return null;

            var window = FromHandle(hWnd);

            if (!window.IsWindowPatternValid())
                return null;

            return window;
        }

        /// <summary>
        /// Get a Window object for Desktop window
        /// </summary>
        public static Window GetDesktopWindow()
        {
            return FromHandle(NativeMethods.GetDesktopWindow());
        }

        public static void Broadcast(uint message)
        {
            NativeMethods.PostMessageW(HWND_BROADCAST, message, IntPtr.Zero, IntPtr.Zero);
        }

        public static uint RegisterWindowMessage(string message)
        {
            return NativeMethods.RegisterWindowMessageW(message);
        }

        #region Window Enumerator

        // used for calling EnumChildWindows in ForEachChildWindow method

        private class WindowEnumeratorEventArgs : EventArgs
        {
            public IntPtr WindowHandle { get; set; }
        }

        private static EnumWindowsProc _windowEnumerator = new(WindowEnumeratorCallback);

        private static event EventHandler<WindowEnumeratorEventArgs>? EnumerateItem;

        private static bool WindowEnumeratorCallback([In] IntPtr hwnd, [In] IntPtr lParam)
        {
            EnumerateItem?.Invoke(null, new WindowEnumeratorEventArgs { WindowHandle = hwnd });
            return true;
        }

        #endregion

        private bool _isDisposing = false;

        /// <summary>
        /// set window title for calling <see cref="Activate"/> method
        /// </summary>
        public string Title { get; set; } = "";

        public IntPtr Handle { get; protected set; } = IntPtr.Zero;

        /// <summary>
        /// Handle of parent window
        /// </summary>
        public IntPtr Parent { get; set; } = IntPtr.Zero;

        /// <summary>
        /// Handle of window handle owner instance
        /// </summary>
        public IntPtr InstanceHandle { get; private set; } = Application.InstanceHandle;

        /// <summary>
        /// window location for calling <see cref="Activate"/> and <see cref="SetWindowPos"/>
        /// </summary>
        public int X { get; set; } = CW_USEDEFAULT;

        /// <summary>
        /// window location for calling <see cref="Activate"/> and <see cref="SetWindowPos"/>
        /// </summary>
        public int Y { get; set; } = CW_USEDEFAULT;

        /// <summary>
        /// window size for calling <see cref="Activate"/> and <see cref="SetWindowPos"/>
        /// </summary>
        public int Width { get; set; } = CW_USEDEFAULT;

        /// <summary>
        /// window size for calling <see cref="Activate"/> and <see cref="SetWindowPos"/>
        /// </summary>
        public int Height { get; set; } = CW_USEDEFAULT;

        /// <summary>
        /// scale factor to multiply window size by it
        /// </summary>
        public float ScaleFactor { get; private set; } = 1;

        public float ScaledWidth => Width * ScaleFactor;

        public float ScaledHeight => Height * ScaleFactor;

        public WindowStyles Style { get; set; } = 0;

        public WindowStylesEx StyleEx { get; set; } = 0;

        public WindowClass? Class { get; protected set; }

        /// <summary>
        /// Load an icon for window and return its handle. Icon will be destroyed when window is disposing.
        /// </summary>
        protected virtual IntPtr LoadIcon()
        {
            return LoadIcon(InstanceHandle, $"#{Macros.IDI_APPLICATION_ICON}");
        }

        /// <summary>
        /// Load an Icon from a module
        /// </summary>
        /// <param name="module">Module handle</param>
        /// <param name="resourceId">Icon resource ID</param>
        /// <returns>Icon handle</returns>
        protected virtual IntPtr LoadIcon(IntPtr module, string resourceId)
        {
            var result = NativeMethods.LoadIconW(module, resourceId);
            if (result == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return result;
        }

        /// <summary>
        /// Load and Get a handle to specified module.
        /// </summary>
        protected IntPtr GetModule(string moduleName)
        {
            var module = NativeMethods.GetModuleHandleW(moduleName);
            if (module == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                if (error == ERROR_MOD_NOT_FOUND)
                {
                    module = NativeMethods.LoadLibraryW(moduleName);
                    if (module == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    throw new Win32Exception(error);
                }
            }
            return module;
        }

        /// <summary>
        /// Register window class and store it in <see cref="Class"/>
        /// </summary>
        protected virtual void RegisterClass()
        {
            var icon = LoadIcon();
            Class = new WindowClass(InstanceHandle, $"{GetType().Name}+{Guid.NewGuid()}", WndProc, icon);
        }

        /// <summary>
        /// Create window and it store its handle in <see cref="Handle"/>
        /// </summary>
        protected virtual void CreateWindow()
        {
            Handle = NativeMethods.CreateWindowExW(
                StyleEx,
                Class!.Atom,
                Title,
                Style,
                X, Y, Width, Height,
                Parent,
                IntPtr.Zero,
                InstanceHandle);

            if (Handle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// register class and create window then scale it
        /// </summary>
        public virtual void Activate()
        {
            if (Handle != IntPtr.Zero) return;

            RegisterClass();
            CreateWindow();

            var windowRect = GetWindowRect();
            X = windowRect.X;
            Y = windowRect.Y;
            Width = windowRect.Width;
            Height = windowRect.Height;

            UpdateScaleFactor();
            UpdateSize();
        }

        public void ShowWindow()
        {
            ShowWindow(ShowWindowCommand.SW_SHOW);
        }

        public virtual void ShowWindow(ShowWindowCommand command)
        {
            NativeMethods.ShowWindow(Handle, command);
        }

        /// <summary>
        /// send WM_CLOSE message to window
        /// </summary>
        public virtual void Close()
        {
            if (!NativeMethods.PostMessageW(Handle, (uint)WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Destroy window and icon object, and unregister class
        /// </summary>
        public virtual void Dispose()
        {
            if (_isDisposing || Class == null) return;
            _isDisposing = true;
            _ = NativeMethods.DestroyWindow(Handle);
            Handle = IntPtr.Zero;
            Class.Dispose();
            _ = NativeMethods.DestroyIcon(Class.Icon);
        }

        /// <summary>
        /// Update value of <see cref="ScaleFactor"/>, call in WM_DPICHANGED
        /// </summary>
        public void UpdateScaleFactor()
        {
            ScaleFactor = ((float)NativeMethods.GetDpiForWindow(Handle)) / USER_DEFAULT_SCREEN_DPI;
        }

        /// <summary>
        /// Update actual size of window, call after changing <see cref="Width"/> or <see cref="Height"/>, or after <see cref="UpdateScaleFactor"/>.
        /// </summary>
        public void UpdateSize()
        {
            SetWindowPosScaled(IntPtr.Zero, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOMOVE);
        }

        /// <summary>
        /// Update actual position of window, call after changing <see cref="X"/> or <see cref="Y"/>.
        /// </summary>
        public void UpdatePosition()
        {
            SetWindowPosScaled(IntPtr.Zero, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);
        }

        public void SetForegroundWindow()
        {
            NativeMethods.SetForegroundWindow(Handle);
        }

        public void SetWindowPos(IntPtr hWndInsertAfter, SetWindowPosFlags flags)
        {
            if (!NativeMethods.SetWindowPos(Handle, hWndInsertAfter, X, Y, Width, Height, flags))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void SetWindowPosScaled(IntPtr hWndInsertAfter, SetWindowPosFlags flags)
        {
            if (!NativeMethods.SetWindowPos(Handle, hWndInsertAfter, X, Y, (int)ScaledWidth, (int)ScaledHeight, flags))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public RECT GetClientRect()
        {
            if (!NativeMethods.GetClientRect(Handle, out var lpRect))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return lpRect;
        }

        public RECT GetWindowRect()
        {
            if (!NativeMethods.GetWindowRect(Handle, out var lpRect))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return lpRect;
        }

        /// <summary>
        /// Center window to given parent window.
        /// </summary>
        public void CenterToWindow(Window parent)
        {
            var parentRect = parent.GetWindowRect();
            X = parentRect.X + (parentRect.Width - Width) / 2;
            Y = parentRect.Y + (parentRect.Height - Height) / 2;
        }

        /// <summary>
        /// Same as <see cref="CenterToWindow(Window)"/> but using <see cref="ScaledWidth"/> and <see cref="ScaledHeight"/>.
        /// </summary>
        /// <param name="parent"></param>
        public void CenterToWindowScaled(Window parent)
        {
            var parentRect = parent.GetWindowRect();
            X = parentRect.X + (int)((parentRect.Width - ScaledWidth) / 2);
            Y = parentRect.Y + (int)((parentRect.Height - ScaledHeight) / 2);
        }

        public string GetText()
        {
            var result = new StringBuilder(Macros.MAX_PATH);
            NativeMethods.GetWindowText(Handle, result, Macros.MAX_PATH);
            return result.ToString();
        }

        public WindowStyles GetWindowStyle()
        {
            var style = NativeMethods.GetWindowLongPtrW(Handle, GWL_STYLE);
            if (style == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new Win32Exception(error);
            }
            return (WindowStyles)unchecked((uint)style.ToInt64());
        }

        public WindowStylesEx GetWindowExStyle()
        {
            var style = NativeMethods.GetWindowLongPtrW(Handle, GWL_EXSTYLE);
            if (style == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new Win32Exception(error);
            }
            return (WindowStylesEx)unchecked((uint)style.ToInt64());
        }

        /// <summary>
        /// Enumerate children of window
        /// </summary>
        public void ForEachChild(Action<Window> callback)
        {
            void ForEachChild(object sender, WindowEnumeratorEventArgs args)
            {
                if (args.WindowHandle == IntPtr.Zero)
                    return;

                if (!NativeMethods.IsWindow(args.WindowHandle))
                    return;

                callback?.Invoke(FromHandle(args.WindowHandle));
            }

            EnumerateItem += ForEachChild;
            NativeMethods.EnumChildWindows(Handle, _windowEnumerator, IntPtr.Zero);
            EnumerateItem -= ForEachChild;
        }

        /// <summary>
        /// Check if current window is a valid UI Automation Window Pattern
        /// Check https://github.com/dotnet/wpf/blob/8db5256603b0fbd3abd588a7eb2c33371d9bd1f1/src/Microsoft.DotNet.Wpf/src/UIAutomation/UIAutomationClient/MS/Internal/Automation/HwndProxyElementProvider.cs#L1028
        /// </summary>
        public bool IsWindowPatternValid()
        {
            StyleEx = GetWindowExStyle();

            if (StyleEx.HasFlag(WindowStylesEx.WS_EX_APPWINDOW))
                return true;

            if (StyleEx.HasFlag(WindowStylesEx.WS_EX_NOACTIVATE))
                return false;

            if (!NativeMethods.IsTopLevelWindow(Handle))
                return false;

            Style = GetWindowStyle();
            
            if (Style == 0)
                return false;

            var hasTitleBar = Style.HasFlag(WindowStyles.WS_BORDER) &&
                Style.HasFlag(WindowStyles.WS_DLGFRAME);

            if (StyleEx.HasFlag(WindowStylesEx.WS_EX_TOOLWINDOW) && !hasTitleBar)
                return false;

            if (Style.HasFlag(WindowStyles.WS_POPUP) && !hasTitleBar)
                return false;

            return true;
        }

        public uint GetProcessId()
        {
            NativeMethods.GetWindowThreadProcessId(Handle, out var pid);
            return pid;
        }

        public bool IsVisible()
        {
            return NativeMethods.IsWindowVisible(Handle);
        }

        public void EnableWindowThemeAttribute(WTNCA attributes)
        {
            var wtaOptions = new WTA_OPTIONS
            {
                Flags = attributes,
                Mask = (uint)attributes,
            };
            NativeMethods.SetWindowThemeAttribute(Handle, WINDOWTHEMEATTRIBUTETYPE.WTA_NONCLIENT, wtaOptions, (uint)Marshal.SizeOf(wtaOptions));
        }

        public void PostMessage(WindowMessage message)
        {
            NativeMethods.PostMessageW(Handle, (uint)message, IntPtr.Zero, IntPtr.Zero);
        }

        public WINDOWPLACEMENT GetWindowPlacement()
        {
            var result = WINDOWPLACEMENT.Default;
            NativeMethods.GetWindowPlacement(Handle, ref result);
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new Win32Exception(error);
            return result;
        }

        public void SetWindowPlacement(WINDOWPLACEMENT windowPlacement)
        {
            NativeMethods.SetWindowPlacement(Handle, ref windowPlacement);
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
                throw new Win32Exception(error);
        }

        /// <summary>
        /// Enable or Disable window
        /// </summary>
        public void SetEnable(bool enabled)
        {
            NativeMethods.EnableWindow(Handle, enabled);
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, uint umsg, IntPtr wParam, IntPtr lParam)
        {
            switch ((WindowMessage)umsg)
            {
                case WindowMessage.WM_CREATE:
                    OnCreate(hwnd);
                    break;

                case WindowMessage.WM_DESTROY:
                    OnDestroy(hwnd);
                    break;

                case WindowMessage.WM_SIZE:
                    OnSizeChanged(hwnd);
                    break;

                case WindowMessage.WM_DPICHANGED:
                    OnDpiChanged(hwnd);
                    break;
            }
            return NativeMethods.DefWindowProcW(hwnd, umsg, wParam, lParam);
        }

        protected void OnCreate(IntPtr hwnd)
        {
            Handle = hwnd;
            Create?.Invoke(this, new WndProcEventArgs(hwnd));
        }

        protected void OnDestroy(IntPtr hwnd)
        {
            Destroy?.Invoke(this, new WndProcEventArgs(hwnd));
        }

        protected void OnSizeChanged(IntPtr hwnd)
        {
            SizeChanged?.Invoke(this, new WndProcEventArgs(hwnd));
        }

        protected void OnDpiChanged(IntPtr hwnd)
        {
            UpdateScaleFactor();
            UpdateSize();
            DpiChanged?.Invoke(this, new WndProcEventArgs(hwnd));
        }

        public event EventHandler<WndProcEventArgs>? Create;
        public event EventHandler<WndProcEventArgs>? Destroy;
        public event EventHandler<WndProcEventArgs>? SizeChanged;
        public event EventHandler<WndProcEventArgs>? DpiChanged;
    }
}
