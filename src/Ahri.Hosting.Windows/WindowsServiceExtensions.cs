using Ahri.Hosting.Windows.Internals;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ahri.Hosting.Windows
{
    /// <summary>
    /// Provides `<see cref="EnableWindowsService"/> methods
    /// for the <see cref="IHostBuilder"/> instances.
    /// </summary>
    public static class WindowsServiceExtensions
    {
        /// <summary>
        /// Test whether the operating environment is under windows or not.
        /// </summary>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        private static bool IsWindowsService()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var Parent = WindowsProcesses.GetParentProcess();
                if (Parent != null)
                    return string.Equals("services", Parent.ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Enable the feature that runs the host instance as the windows service.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHostBuilder EnableWindowsService(this IHostBuilder This)
        {
#pragma warning disable CA1416
            if (!IsWindowsService())
                return This;
#pragma warning restore CA1416

            This.ConfigureServices(Services =>
            {
                Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
                Services.AddSingleton<IHostEnvironment, WindowsService>();
            });

            return This;
        }
    }
}
