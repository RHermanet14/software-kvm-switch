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
        private static int margin { get; set; } = 10;
        public static (int width, int height) GetScreenDimensions()
        {
            Screen? primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
                return (0, 0);
            Rectangle screenBounds = primaryScreen.Bounds;
            return (screenBounds.Width, screenBounds.Height);
        }

    }
}