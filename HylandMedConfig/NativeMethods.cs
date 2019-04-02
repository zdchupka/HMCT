using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HylandMedConfig
{
	public static class NativeMethods
	{
		[DllImport( "KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall )]
		internal static extern bool SetProcessWorkingSetSize( IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize );

		[DllImport( "KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall )]
        internal static extern IntPtr GetCurrentProcess();

		[DllImport( "gdi32.dll", SetLastError = true )]
        internal static extern bool DeleteObject( IntPtr hObject );

        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
