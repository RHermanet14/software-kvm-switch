using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace services
{
    public class MouseService : NativeWindow
    {
        private const uint RID_INPUT = 0x10000003;
        private static bool _registered = false;
        private DateTime _lastUpdateTime = DateTime.Now;
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
        private static extern uint GetRawInputData( // For WndProc
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        public bool StartTracking()
        {
            if (_registered)
                return true;
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = 0;
            devices[0].hwndTarget = IntPtr.Zero; // Might need to add a handle
            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
                return false;
            _registered = true;
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_INPUT = 0x00FF;
            if (m.Msg == WM_INPUT && _registered)
            {
                uint size = 0;
                GetRawInputData(m.LParam, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                if (size <= 0)
                {
                    base.WndProc(ref m);
                    return;
                }
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    uint actualSize = GetRawInputData(m.LParam, RID_INPUT, buffer, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                    if (actualSize == size)
                    {
                        RAWINPUT rawInput = Marshal.PtrToStructure<RAWINPUT>(buffer);
                        if (rawInput.header.dwType == 0) // RIM_TYPEMOUSE
                        {
                            //UpdateMovementData(rawInput.mouse.lLastX, rawInput.mouse.lLastY);
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
        
/*
        private void UpdateMovementData(int deltaX, int deltaY)
        {
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

        public void Dispose()
        {
            if (!_disposed)
            {
                StopTracking();
                DestroyHandle();
                _disposed = true;
            }
        }
    }
*/
    }
}

