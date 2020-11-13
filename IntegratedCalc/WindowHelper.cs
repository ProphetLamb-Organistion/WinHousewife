using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace IntegratedCalc
{
    public static class WindowHelper
    {
        public const int SnapGap = 30;
        public const int Padding = 0;

        [Flags]
        public enum SnapOrigins
        {
            None = 0x00,
            Left = 0x01,
            Top = 0x02,
            Right = 0x04,
            Bottom = 0x08
        }

        public static SnapOrigins GetSnapOrigins(Window window) => GetSnapOrigins(window, Screen.FromHandle(new WindowInteropHelper(window).Handle), SnapGap);

        public static SnapOrigins GetSnapOrigins(Window window, Screen screen, double snapGap)
        {
            var origins = SnapOrigins.None;
            if (Math.Abs(window.Left - screen.WorkingArea.Left) <= snapGap)
                origins |= SnapOrigins.Left;
            if (Math.Abs(window.Top - screen.WorkingArea.Top) <= snapGap)
                origins |= SnapOrigins.Top;
            if (Math.Abs((window.Left + window.Width) - (screen.WorkingArea.Left + screen.WorkingArea.Width)) <= snapGap)
                origins |= SnapOrigins.Right;
            if (Math.Abs((window.Top + window.Height) - (screen.WorkingArea.Top + screen.WorkingArea.Height)) <= snapGap)
                origins |= SnapOrigins.Bottom;
            return origins;
        }

        public static void SnapToOrigins(Window window, SnapOrigins origins) => SnapToOrigins(window, Screen.FromHandle(new WindowInteropHelper(window).Handle), origins, 5);

        public static void SnapToOrigins(Window window, Screen screen, SnapOrigins origins, double padding)
        {
            double winL = window.Left, winT = window.Top;
            if ((origins & SnapOrigins.Left) != 0)
            {
                winL = screen.WorkingArea.Left + padding;
            }
            if ((origins & SnapOrigins.Top) != 0)
            {
                winT = screen.WorkingArea.Top + padding;
            }
            if ((origins & SnapOrigins.Right) != 0)
            {
                winL = screen.WorkingArea.Left + screen.WorkingArea.Width - window.Width - padding;
            }
            if ((origins & SnapOrigins.Bottom) != 0)
            {
                winT = screen.WorkingArea.Top + screen.WorkingArea.Height - window.Height - padding;
            }
            window.Left = winL;
            window.Top = winT;
        }

        public static void ResizeFitToScreen(Window window, double width, double height)
        {
            Screen monInfo = Screen.FromHandle(new WindowInteropHelper(window).Handle);
            // Add padding to workarea
            double workAreaL = monInfo.WorkingArea.Left + Padding,
                workAreaT = monInfo.WorkingArea.Top + Padding,
                workAreaW = monInfo.WorkingArea.Right - monInfo.WorkingArea.Left - 2 * Padding,
                workAreaH = monInfo.WorkingArea.Bottom - monInfo.WorkingArea.Top - 2 * Padding;
            // Client area
            double clientAreaL = window.Left,
                clientAreaT = window.Top,
                clientAreaW = width,
                clientAreaH = height;
            // Client/working area overlap
            double areaOverlapL = workAreaL - clientAreaL,
                areaOverlapT = workAreaT - clientAreaT,
                areaOverlapR = (clientAreaL + clientAreaW) - (workAreaL + workAreaW),
                areaOverlapB = (clientAreaT + clientAreaH) - (workAreaT + workAreaH);
            // Clamp client size to working area
            double clampW = 0, clampH = 0;
            if (areaOverlapL > 0)
            {
                clientAreaL += areaOverlapL;
                clientAreaW -= areaOverlapL;
                clampW = areaOverlapL;
            }
            if (areaOverlapT > 0)
            {
                clientAreaT += areaOverlapT;
                clientAreaH -= areaOverlapT;
                clampH = areaOverlapT;
            }
            if (areaOverlapR > 0)
            {
                clientAreaW -= areaOverlapR;
                clampW = areaOverlapR;
            }
            if (areaOverlapB > 0)
            {
                clientAreaH -= areaOverlapB;
                clampH = areaOverlapB;
            }
            // Try to restore to original size
            if (areaOverlapL < 0)
            {
                double offset = Math.Min(clampW, -areaOverlapL);
                clampW -= offset;
                clientAreaL -= offset;
                clientAreaW += offset;
            }
            if (areaOverlapT < 0)
            {
                double offset = Math.Min(clampH, -areaOverlapT);
                clampH -= offset;
                clientAreaT -= offset;
                clientAreaH += offset;
            }
            if (areaOverlapR < 0)
            {
                double offset = Math.Min(clampW, -areaOverlapR);
                clampW -= offset;
                clientAreaW += offset;
            }
            if (areaOverlapB < 0)
            {
                double offset = Math.Min(clampH, -areaOverlapB);
                clampH -= offset;
                clientAreaH += offset;
            }
            window.Left = clientAreaL;
            window.Top = clientAreaT;
            window.Width = clientAreaW;
            window.Height = clientAreaH;
        }

        #region PInvoke
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion
    }
}
