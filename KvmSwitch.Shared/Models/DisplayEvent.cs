using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
namespace Shared {
    public static class DisplayEvent
    {
        private static int margin;
        public static (int width, int height) GetScreenDimensions()
        {
            Screen? primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
                return (0, 0);
            Rectangle screenBounds = primaryScreen.Bounds;
            return (screenBounds.Width, screenBounds.Height);
        }

        public static void SetScreenMargin(int NewMargin) { margin = NewMargin; }
        public static int GetScreenMargin() { return margin; }
    }
}