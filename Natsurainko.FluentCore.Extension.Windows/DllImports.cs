using System;
using System.Runtime.InteropServices;

namespace Natsurainko.FluentCore.Extension.Windows
{
    public class DllImports
    {
        /// <summary>
        /// Changes the text of the specified window's title bar (if it has one). If the specified window is a control, the text of the control is changed. 
        /// <para>
        /// However, SetWindowText cannot change the text of a control in another application.
        /// </para>
        /// </summary>
        /// <param name="hWnd">A handle to the window or control whose text is to be changed.</param>
        /// <param name="lpString">The new title or control text.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// <para>
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </para>
        /// </returns>
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);
    }
}
