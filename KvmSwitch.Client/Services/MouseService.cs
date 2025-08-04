using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
        private long _lastDeltaX = 0;
        private long _lastDeltaY = 0;
        private DateTime _lastUpdateTime = DateTime.Now;
        private float _lastVelocityX = 0f;
        private float _lastVelocityY = 0f;
        private readonly int _smoothingFactor = 3;
        private readonly float[] _recentVelocityX;
        private readonly float[] _recentVelocityY;
        private int _velocityIndex = 0;
        #endregion

        #region Public Properties
        public float VelocityX { get; private set; } = 0f;
        public float VelocityY { get; private set; } = 0f;
        public float AccelerationX { get; private set; } = 0f;
        public float AccelerationY { get; private set; } = 0f;
        public long RawDeltaX { get; private set; } = 0;
        public long RawDeltaY { get; private set; } = 0;

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
            public ushort usFlags;
            public ushort usButtonFlags;
            public ushort usButtonData;
            public ulong ulRawButtons;
            public long lLastX;
            public long lLastY;
            public ulong ulExtraInformation;
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
            _recentVelocityX = new float[_smoothingFactor];
            _recentVelocityY = new float[_smoothingFactor];

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
            devices[0].hwndTarget = Handle; // Might need to add a handle
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
                        RAWINPUT rawInput = Marshal.PtrToStructure<RAWINPUT>(buffer);
                        if (rawInput.header.dwType == RIM_TYPEMOUSE)
                        {
                            UpdateMovementData(rawInput.mouse.lLastX, rawInput.mouse.lLastY);
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

        private void UpdateMovementData(long deltaX, long deltaY)
        {
            Console.WriteLine($"Raw movement: deltaX={deltaX}, deltaY={deltaY}");
            DateTime currentTime = DateTime.Now;
            double timeDelta = (currentTime - _lastUpdateTime).TotalMilliseconds;

            if (timeDelta > 0)
            {
                // Store raw delta values
                RawDeltaX = deltaX;
                RawDeltaY = deltaY;

                // Calculate velocity (pixels per second)
                float currentVelocityX = (float)(deltaX / (timeDelta / 1000.0));
                float currentVelocityY = (float)(deltaY / (timeDelta / 1000.0));

                // Apply smoothing to velocity
                _recentVelocityX[_velocityIndex] = currentVelocityX;
                _recentVelocityY[_velocityIndex] = currentVelocityY;
                _velocityIndex = (_velocityIndex + 1) % _smoothingFactor;

                // Calculate smoothed velocity
                float smoothedVelocityX = 0f;
                float smoothedVelocityY = 0f;
                for (int i = 0; i < _smoothingFactor; i++)
                {
                    smoothedVelocityX += _recentVelocityX[i];
                    smoothedVelocityY += _recentVelocityY[i];
                }
                VelocityX = smoothedVelocityX / _smoothingFactor;
                VelocityY = smoothedVelocityY / _smoothingFactor;

                // Calculate acceleration (change in velocity per second)
                AccelerationX = (float)((VelocityX - _lastVelocityX) / (timeDelta / 1000.0));
                AccelerationY = (float)((VelocityY - _lastVelocityY) / (timeDelta / 1000.0));

                // Update tracking variables
                _lastDeltaX = deltaX;
                _lastDeltaY = deltaY;
                _lastVelocityX = VelocityX;
                _lastVelocityY = VelocityY;
                _lastUpdateTime = currentTime;

                // Fire event
                MouseMovement?.Invoke(this, new MouseMovementEventArgs
                {
                    DeltaX = deltaX,
                    DeltaY = deltaY,
                    VelocityX = VelocityX,
                    VelocityY = VelocityY,
                    AccelerationX = AccelerationX,
                    AccelerationY = AccelerationY,
                    Timestamp = currentTime
                });
            }
        }

        private void ResetValues()
        {
            VelocityX = VelocityY = 0f;
            AccelerationX = AccelerationY = 0f;
            RawDeltaX = RawDeltaY = 0;
            _lastVelocityX = _lastVelocityY = 0f;
            _lastDeltaX = _lastDeltaY = 0;

            for (int i = 0; i < _smoothingFactor; i++)
            {
                _recentVelocityX[i] = 0f;
                _recentVelocityY[i] = 0f;
            }
            _velocityIndex = 0;
        }
        public bool StopTracking()
        {
            if (!_registered) return true;

            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = RIDEV_REMOVE;
            devices[0].hwndTarget = IntPtr.Zero;

            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                Console.WriteLine("Error: Unsuccessfully unregistered device");
                return false;
            }
            _registered = false;
            ResetValues();
            return true;
        }
        public void Dispose()
        {
            Console.WriteLine("Cleaning up device...");
            if (!_disposed)
            {
                StopTracking();
                DestroyHandle();
                _disposed = true;
            }
        }
    }
    public class MouseMovementEventArgs : EventArgs
    {
        public long DeltaX { get; set; }
        public long DeltaY { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

