using System;
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
        private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        private const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
        private const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
        private const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
        private const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;
        private const ushort RI_MOUSE_WHEEL = 0x0400; // Uses usButtonData to determine distance
        private const ushort RI_MOUSE_HWHEEL = 0x0800; // Uses usButtonData to determine distance
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
                        
                        if (header.dwType == RIM_TYPEMOUSE)
                        {
                            // Calculate offset to mouse data (after header)
                            int headerSize = Marshal.SizeOf<RAWINPUTHEADER>();
                            IntPtr mouseDataPtr = IntPtr.Add(buffer, headerSize);

                            // Read mouse data directly
                            RAWMOUSE mouseData = Marshal.PtrToStructure<RAWMOUSE>(mouseDataPtr);

                            switch (mouseData.usButtonFlags)
                            {
                                case RI_MOUSE_LEFT_BUTTON_DOWN:
                                    Console.WriteLine("Left mouse button clicked");
                                    break;
                                case RI_MOUSE_LEFT_BUTTON_UP:
                                    Console.WriteLine("Left mouse button released");
                                    break;
                                case RI_MOUSE_RIGHT_BUTTON_DOWN:
                                    Console.WriteLine("right mouse button clicked");
                                    break;
                                case RI_MOUSE_RIGHT_BUTTON_UP:
                                    Console.WriteLine("right mouse button released");
                                    break;
                                case RI_MOUSE_MIDDLE_BUTTON_DOWN:
                                    Console.WriteLine("middle mouse button clicked");
                                    break;
                                case RI_MOUSE_MIDDLE_BUTTON_UP:
                                    Console.WriteLine("middle mouse button released");
                                    break;
                                case RI_MOUSE_BUTTON_4_DOWN:
                                    Console.WriteLine("mouse4 button clicked");
                                    break;
                                case RI_MOUSE_BUTTON_4_UP:
                                    Console.WriteLine("mouse4 button released");
                                    break;
                                case RI_MOUSE_BUTTON_5_DOWN:
                                    Console.WriteLine("mouse5 button clicked");
                                    break;
                                case RI_MOUSE_BUTTON_5_UP:
                                    Console.WriteLine("mouse5 button released");
                                    break;
                                case RI_MOUSE_WHEEL:
                                    if ((short)mouseData.usButtonData < 0)
                                        Console.WriteLine("Scroll down");
                                    else
                                        Console.WriteLine("Scroll up");
                                    break;
                                case RI_MOUSE_HWHEEL:
                                    if ((short)mouseData.usButtonData < 0)
                                        Console.WriteLine("Scroll left");
                                    else
                                        Console.WriteLine("Scroll right");
                                    break;   
                            }

                            UpdateMovementData(mouseData.usButtonFlags, mouseData.usButtonData, mouseData.lLastX, mouseData.lLastY);
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

            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_REMOVE;
            devices[0].hwndTarget = IntPtr.Zero;

            bool success = RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
            
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
    
}