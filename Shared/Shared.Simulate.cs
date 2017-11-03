using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Shared
{
    class HelperSimulator
    {
        public static IEnumerable<IntPtr> GetQQFFOHandle()
        {
            return Process.GetProcesses().Where(i => i.ProcessName.StartsWith("qqffo")).Select(i => i.MainWindowHandle);
        }
        public static IEnumerable<IntPtr> GetBaiduNetHandle()
        {
            return Process.GetProcesses().Where(i => i.ProcessName.ToLower().StartsWith("baidunetdisk")).Select(i => i.MainWindowHandle);
        }
        public static IEnumerable<string> GetMainWindowHandles(string patternFilter = "*")
        {
            if (patternFilter == "*")
                return Process.GetProcesses().Where(i => i.MainWindowHandle.ToInt32() > 0).Select(i => string.Format("{0}|{1}", i.ProcessName, i.MainWindowHandle));
            else
                return Process.GetProcesses().Where(i => Regex.IsMatch(i.ProcessName, patternFilter)).Where(i => i.MainWindowHandle.ToInt32() > 0).Select(i => string.Format("{0}|{1}", i.ProcessName, i.MainWindowHandle));
        }
        public static (int, int) GetCursorPosition()
        {
            POINT lpPoint;
            Win32.GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return (lpPoint.X, lpPoint.Y);
        }

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private const int MOUSEEVENTF_WHEEL = 0x800;
        public static void MouseLeftClick(int x, int y)
        {
            Win32.SetCursorPos(x, y);

            uint X = (uint)x;
            uint Y = (uint)y;
            Win32.mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);

        }
        public static void MouseRightClick(IntPtr handle, int x, int y)
        {
            //Win32.SetCursorPos(x, y);
            //Random objRandom = new Random();
            //Win32.mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
            //System.Threading.Thread.Sleep(objRandom.Next(1, 332));
            //Win32.mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);

        }
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public static void KeyPress(IntPtr handle, Keys key)
        {

            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)key, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)key, 0);
        }
        public static void KeyPressWithAlt(IntPtr handle, Keys key)
        {
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)Keys.Menu, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)Keys.Menu, 0);
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)Keys.Menu, 0);
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)key, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)key, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)Keys.Menu, 0);
        }
        public static void KeyPressWithCtrl(IntPtr handle, Keys key)
        {
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)Keys.ControlKey, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)Keys.ControlKey, 0);
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)Keys.ControlKey, 0);
            Win32.SendMessage(handle, WM_KEYDOWN, (ushort)key, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)key, 0);
            Win32.SendMessage(handle, WM_KEYUP, (ushort)Keys.ControlKey, 0);
        }
    }
}
