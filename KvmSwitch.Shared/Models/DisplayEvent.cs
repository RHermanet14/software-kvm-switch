using System;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
namespace Shared
{

    public class DisplayEvent : Form
    {
        private static uint RID_INPUT = 0x10000003;
        private static bool raw_input_initialized = false;
        private static int margin;
        public static (int width, int height) GetScreenDimensions()
        {
            Screen? primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
                return (0, 0);
            Rectangle screenBounds = primaryScreen.Bounds;
            return (screenBounds.Width, screenBounds.Height);
        }

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

        [DllImport("user32.dll")] // For the StartTracking function
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        public bool StartTracking()
        {
            if (raw_input_initialized)
                return true;
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = 0;
            devices[0].hwndTarget = IntPtr.Zero;
            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
                return false;
            raw_input_initialized = true;
            return true;
        }

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        protected override void WndProc(ref Message m)
        {
            const int WM_INPUT = 0x00FF;
            switch (m.Msg)
            {
                case WM_INPUT:
                    uint size = 0;
                    GetRawInputData(m.LParam, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
                    if (size <= 0)
                        break;
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
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

        }

        public static void SetScreenMargin(int NewMargin) { margin = NewMargin; }
        public static int GetScreenMargin() { return margin; }
    }
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