using Alphaleonis.Win32.Filesystem;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Util.ScriptExtender
{
	public class UpdaterAPI : IDisposable
	{
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
		static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr libraryReference);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true)]
		private delegate void SEUpdaterInitialize([MarshalAs(UnmanagedType.LPStr)] string configPath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
		private delegate void SESetGameVersion(int major, int minor, int revision, int build);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
		private delegate bool SEUpdate();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
		private delegate void SEUpdaterShowConsole();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
		private delegate void SEUpdaterGetError([MarshalAs(UnmanagedType.LPStr)] ref string buffer, uint length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
		private delegate void SEUpdaterShutdown();

		private readonly SEUpdaterInitialize _initializeUpdaterWrapper;
		private readonly SESetGameVersion _setGameVersionWrapper;
		private readonly SEUpdate _updateWrapper;
		private readonly SEUpdaterShowConsole _updaterShowConsoleWrapper;
		private readonly SEUpdaterGetError _updaterGetErrorWrapper;
		private readonly SEUpdaterShutdown _updaterShutdownWrapper;

		private readonly string _updaterPath;
		private readonly IntPtr _dll;

		private readonly bool _loaded = false;
		public bool IsLoaded => _loaded;

		private bool _consoleIsOpen = false;

#if DEBUG
		/*[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);

		[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymCleanup(IntPtr hProcess);

		[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile,
			 string ImageName, string ModuleName, long BaseOfDll, int DllSize, IntPtr Data, int Flags);

		[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymEnumerateSymbols64(IntPtr hProcess,
		   ulong BaseOfDll, SymEnumerateSymbolsProc64 EnumSymbolsCallback, IntPtr UserContext);

		public delegate bool SymEnumerateSymbolsProc64(string SymbolName,
			  ulong SymbolAddress, uint SymbolSize, IntPtr UserContext);

		public static bool EnumSyms(string name, ulong address, uint size, IntPtr context)
		{
			DivinityApp.Log(name);
			return true;
		}*/
#endif

		private T GetWrapper<T>(string funcName) where T : Delegate
		{
			var address = GetProcAddress(_dll, funcName);
			if(address != IntPtr.Zero)
			{
				return Marshal.GetDelegateForFunctionPointer<T>(address);
			}
			return null;
		}

		[HandleProcessCorruptedStateExceptions]
		private void Init(string configPath)
		{
			try
			{
				_initializeUpdaterWrapper.Invoke(configPath);
			}
			catch (AccessViolationException ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error initializing updater ({errorCode}):\n{ex}");
			}
		}
		
		public UpdaterAPI(string updaterPath, string configPath)
		{
			_updaterPath = updaterPath;
			_dll = LoadLibrary(_updaterPath);

			if (_dll != IntPtr.Zero)
			{
#if DEBUG
				/*IntPtr hCurrentProcess = Process.GetCurrentProcess().Handle;
				var status = SymInitialize(hCurrentProcess, null, false);
				var baseOfDll = SymLoadModuleEx(hCurrentProcess, IntPtr.Zero, updaterPath, null, 0, 0, IntPtr.Zero, 0);
				if (SymEnumerateSymbols64(hCurrentProcess, baseOfDll, EnumSyms, IntPtr.Zero) == false)
				{
					DivinityApp.Log("Failed to enum symbols.");
				}*/
#endif
				_initializeUpdaterWrapper = GetWrapper<SEUpdaterInitialize>("SEUpdaterInitialize");
				_setGameVersionWrapper = GetWrapper<SESetGameVersion>("SESetGameVersion");
				_updateWrapper = GetWrapper<SEUpdate>("SEUpdate");
				_updaterShowConsoleWrapper = GetWrapper<SEUpdaterShowConsole>("SEUpdaterShowConsole");
				_updaterGetErrorWrapper = GetWrapper<SEUpdaterGetError>("SEUpdaterGetError");
				_updaterShutdownWrapper = GetWrapper<SEUpdaterShutdown>("SEUpdaterShutdown");

				_loaded = (_initializeUpdaterWrapper != null && _setGameVersionWrapper != null 
					&& _updateWrapper != null && _updaterGetErrorWrapper != null);
				if (_loaded)
				{
					Init(configPath);
				}
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public bool SetGameVersion(string exePath)
		{
			if (!_loaded) return false;
			try
			{
				if (File.Exists(exePath))
				{
					var fvi = FileVersionInfo.GetVersionInfo(exePath);
					if (fvi != null)
					{
						DivinityApp.Log($"Setting game version to {fvi.FileVersion}");
						_setGameVersionWrapper.Invoke(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error setting game version ({errorCode}):\n{ex}");
			}
			return false;
		}

		[HandleProcessCorruptedStateExceptions]
		public bool ShowConsoleWindow()
		{
			if (!_loaded) return false;
			try
			{
				_updaterShowConsoleWrapper.Invoke();
				_consoleIsOpen = true;
			}
			catch (Exception ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error showing console window ({errorCode}):\n{ex}");
			}
			
			return true;
		}

		[HandleProcessCorruptedStateExceptions]
		public bool Update()
		{
			if (!_loaded) return false;
			try
			{
				_updateWrapper.Invoke();
			}
			catch (Exception ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error updating extender ({errorCode}):\n{ex}");
			}
			
			return true;
		}

		[HandleProcessCorruptedStateExceptions]
		public string GetError()
		{
			var error = "";
			if (!_loaded) return error;
			try
			{
				_updaterGetErrorWrapper.Invoke(ref error, 10);
			}
			catch (Exception ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error getting another error (?) ({errorCode}):\n{ex}");
			}
			return error;
		}

		[HandleProcessCorruptedStateExceptions]
		private void Shutdown()
		{
			if (!_loaded) return;
			try
			{
				DivinityApp.Log("Shutting down the updater.");
				_updaterShutdownWrapper.Invoke();
			}
			catch (Exception ex)
			{
				var errorCode = Marshal.GetLastWin32Error();
				DivinityApp.Log($"Error shutting down updater ({errorCode}):\n{ex}");
			}
		}

		private bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				if (_dll != IntPtr.Zero)
				{
					if (_loaded) Shutdown();
					FreeLibrary(_dll);
				}
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
