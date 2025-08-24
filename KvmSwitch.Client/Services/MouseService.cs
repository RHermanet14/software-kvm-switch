using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Shared;
namespace services
{
    public class MouseService : NativeWindow, IDisposable
    {
        public event EventHandler<MouseMovementEventArgs>? MouseMovement;

        #region Private Variables
        private const uint RID_INPUT = 0x10000003;
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RIDEV_REMOVE = 0x00000001;
        private const uint RIDEV_INPUTSINK = 0x00000100;
        private const int WM_INPUT = 0x00FF;
        private bool _registered = false;
        private bool _disposed = false;
        #endregion
        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWMOUSE
        {
            public uint usFlags;
            public ushort usButtonFlags;
            public ushort usButtonData;
            public uint ulRawButtons; // Changed from ulong to uint
            public int lLastX;  // Changed from long to int
            public int lLastY;  // Changed from long to int
            public uint ulExtraInformation; // Changed from ulong to uint
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse; // union { RAWMOUSE mouse; RAWKEYBOARD keyboard; RAWHID hid; }
        }
        #endregion

        [DllImport("user32.dll")] // For the StartTracking function
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll")]
        private static extern int GetRawInputData( // For WndProc
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        public MouseService()
        {
            // Create a hidden window for receiving raw input messages
            CreateHandle(new CreateParams
            {
                Caption = "RawMouseTracker",
                ClassName = "STATIC",
                Style = 0,
                ExStyle = 0,
                Height = 0,
                Width = 0,
                Parent = IntPtr.Zero
            });
        }

        public bool StartTracking()
        {
            if (_registered)
                return true;
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_INPUTSINK;
            devices[0].hwndTarget = Handle;

            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                Console.WriteLine("Failed to register device");
                return false;
            }
            _registered = true;
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT && _registered)
            {
                uint size = 0;
                if (GetRawInputData(m.LParam, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == -1)
                {
                    Console.WriteLine("GetRawInputData failed");
                    base.WndProc(ref m);
                    return;
                }
                if (size <= 0)
                {
                    Console.WriteLine("Size not allocated");
                    base.WndProc(ref m);
                    return;
                }
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    int actualSize = GetRawInputData(m.LParam, RID_INPUT, buffer, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                    if (actualSize == size)
                    {
                        // Read the header first
                        RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);
                        int headerSize = Marshal.SizeOf<RAWINPUTHEADER>();
                        IntPtr DataPtr = IntPtr.Add(buffer, headerSize);

                        if (header.dwType == RIM_TYPEMOUSE)
                        {
                            RAWMOUSE mouseData = Marshal.PtrToStructure<RAWMOUSE>(DataPtr);
                            MouseMovement?.Invoke(this, new MouseMovementEventArgs
                            {
                                ClickType = mouseData.usButtonFlags,
                                ScrollSpeed = (short)mouseData.usButtonData,
                                VelocityX = mouseData.lLastX,
                                VelocityY = mouseData.lLastY,
                            });
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            base.WndProc(ref m);
        }

        public bool StopTracking()
        {
            if (!_registered) return true;

            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_REMOVE;
            devices[0].hwndTarget = IntPtr.Zero;

            bool success = RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
            if (success)
                _registered = false;
            return success;
        }
        public void Dispose()
        {
            Console.WriteLine("Successfully entered dispose function");
            if (!_disposed)
            {
                StopTracking();
                DestroyHandle();
                _disposed = true;
            }
        }
    }
    public class SuppressionService : IDisposable
    {
        public static event EventHandler<KeyboardInputEventArgs>? KeyboardInput;
        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private readonly HOOKPROC _mouseProc;
        private readonly HOOKPROC _keyboardProc;
        private IntPtr _mouseHookID = IntPtr.Zero;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private static volatile bool _suppressMouse = false;
        private static volatile bool _suppressKeyboard = false;
        public delegate IntPtr HOOKPROC(int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        public SuppressionService()
        {
            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;

            _mouseHookID = SetMouseHook(_mouseProc);
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
        }
        public void StartSuppression() { _suppressMouse = true; _suppressKeyboard = true; }
        public void StopSuppression() { _suppressMouse = false; _suppressKeyboard = false; }
        private IntPtr SetMouseHook(HOOKPROC proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, 
                    GetModuleHandle(curModule.ModuleName!), 0); 
                if (hook == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to install mouse hook. Error: {error}");
                } 
                return hook;
            }
        }
        private IntPtr SetKeyboardHook(HOOKPROC proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                IntPtr hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, 
                    GetModuleHandle(curModule.ModuleName!), 0);
                if (hook == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to install keyboard hook. Error: {error}");
                } 
                return hook;
            }
        }
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    if (_suppressMouse)
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mouse hook error: {ex.Message}");
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    KBDLLHOOKSTRUCT kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                    KeyboardInput?.Invoke(null, new KeyboardInputEventArgs
                    {
                        Key = (ushort)kb.scanCode,
                        KeyInputType = (ushort)wParam,
                    });
                    if (_suppressKeyboard)
                        return 1;
                        /*Console.WriteLine($"NOT suppressing keyboard input: wParam={wParam}");*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keyboard hook error: {ex.Message}");
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    
        public void Dispose()
        {     
            StopSuppression(); 
            if (_mouseHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookID);
                _mouseHookID = IntPtr.Zero;
            }
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
                _keyboardHookID = IntPtr.Zero;
            }
        }
    }
}