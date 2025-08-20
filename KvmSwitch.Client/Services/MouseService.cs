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
        private const uint RIM_TYPEKEYBOARD = 1;
        private const uint RIDEV_REMOVE = 0x00000001;
        private const uint RIDEV_INPUTSINK = 0x00000100;
        private const int WM_INPUT = 0x00FF;
        
        private bool _registered = false;
        private bool _disposed = false;
        private DateTime _lastUpdateTime = DateTime.Now;
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
            public RAWKEYBOARD keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public ulong ExtraInformation;
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
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[2];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_INPUTSINK;
            devices[0].hwndTarget = Handle;

            devices[1].usUsagePage = 0x01;
            devices[1].usUsage = 0x06;
            devices[1].dwFlags = RIDEV_INPUTSINK;
            devices[1].hwndTarget = Handle;
            if (!RegisterRawInputDevices(devices, 2, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
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
                            // Calculate offset to mouse data (after header)
                            //int headerSize = Marshal.SizeOf<RAWINPUTHEADER>();
                            //IntPtr mouseDataPtr = IntPtr.Add(buffer, headerSize);

                            // Read mouse data directly
                            RAWMOUSE mouseData = Marshal.PtrToStructure<RAWMOUSE>(DataPtr);

                            UpdateMovementData(mouseData.usButtonFlags, mouseData.usButtonData, mouseData.lLastX, mouseData.lLastY);
                        }
                        else if (header.dwType == RIM_TYPEKEYBOARD)
                        {
                            RAWKEYBOARD keyboardData = Marshal.PtrToStructure<RAWKEYBOARD>(DataPtr);
                            Console.WriteLine($"{keyboardData.MakeCode}, {keyboardData.Flags}");
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

        private void UpdateMovementData(uint flags, ushort data, int deltaX, int deltaY)
        {
            DateTime currentTime = DateTime.Now;
            double timeDelta = (currentTime - _lastUpdateTime).TotalMilliseconds;

            if (timeDelta > 0)
            {
                // Fire event
                MouseMovement?.Invoke(this, new MouseMovementEventArgs
                {
                    ClickType = flags,
                    ScrollSpeed = (short)data,
                    VelocityX = deltaX,
                    VelocityY = deltaY,
                    TimeDelta = timeDelta / 1000,
                });
            }
        }

        public bool StopTracking()
        {
            if (!_registered) return true;

            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[2];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_REMOVE;
            devices[0].hwndTarget = IntPtr.Zero;

            devices[1].usUsagePage = 0x01;
            devices[1].usUsage = 0x06;
            devices[1].dwFlags = RIDEV_REMOVE;
            devices[1].hwndTarget = IntPtr.Zero;
            bool success = RegisterRawInputDevices(devices, 2, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
            
            if (success)
            {
                _registered = false;
            }

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
    public class MouseSuppressionService : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private readonly HOOKPROC _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private static volatile bool _suppressMouse = false;
        public delegate IntPtr HOOKPROC(int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public MouseSuppressionService()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }
        public void StartSuppression() { _suppressMouse = true; }
        public void StopSuppression() { _suppressMouse = false; }
        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
        private IntPtr SetHook(HOOKPROC proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName!), 0);
            }
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    if (_suppressMouse)
                    {
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hook error: {ex.Message}");
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}