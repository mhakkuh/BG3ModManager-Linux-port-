using System.Runtime.InteropServices;

namespace Toolbox
{
	public static partial class NativeMethods
	{
		[LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryA")]
		public static partial IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

		[LibraryImport("kernel32.dll", SetLastError = true)]
		public static partial IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procedureName);

		[LibraryImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool FreeLibrary(IntPtr libraryReference);

		public static void ThrowExceptionForLastWin32Error()
		{
			var errorCode = Marshal.GetHRForLastWin32Error();
			Marshal.ThrowExceptionForHR(errorCode);
		}
	}
}
