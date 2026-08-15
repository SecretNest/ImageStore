using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SecretNest.ImageStore
{
    /// <summary>
    /// Closes the database when the module goes away.
    /// </summary>
    /// <remarks>
    /// Two separate hooks are needed, because neither covers the other:
    ///
    /// OnRemove fires for Remove-ImageStoreDatabase's module, that is Remove-Module.
    /// It does not fire when the host simply exits.
    ///
    /// ProcessExit fires on a normal host exit, which is how a session actually
    /// ends most of the time.
    ///
    /// Both funnel into DatabaseConnection.Close, which is idempotent and locked, so
    /// firing twice - or racing a Close-ImageStoreDatabase on the pipeline thread -
    /// is harmless. Neither hook runs if the process is killed; SQLite recovers from
    /// that on its own, the cost being the -wal and -shm files left behind.
    /// </remarks>
    public class ModuleLifetime : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        static bool _processExitHooked;
        static readonly object _lock = new object();

        /// <summary>
        /// Directory the module was loaded from.
        /// </summary>
        /// <remarks>
        /// Not AppContext.BaseDirectory: for a module loaded by Import-Module that
        /// is the host's directory - where pwsh.exe lives - not this one. Anything
        /// looking for a file shipped beside the module has to start here.
        /// </remarks>
        internal static string Directory
        {
            get { return Path.GetDirectoryName(typeof(ModuleLifetime).Assembly.Location); }
        }

        public void OnImport()
        {
            PreloadSqliteEngine();

            lock (_lock)
            {
                //Importing twice in one session must not stack handlers. The
                //assembly is never unloaded, so this static survives Remove-Module
                //followed by a fresh Import-Module.
                if (_processExitHooked)
                    return;

                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                _processExitHooked = true;
            }
        }

        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            DatabaseConnection.Close();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            DatabaseConnection.Close();
        }

        /// <summary>
        /// Loads the native SQLite engine before anything can touch SqliteConnection.
        /// </summary>
        /// <remarks>
        /// Import-Module loads this assembly directly rather than starting an
        /// application, so none of the RID-specific probing that would find
        /// runtimes\&lt;rid&gt;\native happens - that is driven by deps.json when a host
        /// starts an app, and there is no such step here. The P/Invoke inside
        /// SQLitePCLRaw then searches only the ordinary OS paths, does not find
        /// e_sqlite3, and the failure surfaces on the first database cmdlet as
        /// "The type initializer for 'Microsoft.Data.Sqlite.SqliteConnection' threw
        /// an exception", which says nothing about a missing file.
        ///
        /// Loading it by full path here is enough: Windows keys loaded modules by
        /// name, so the P/Invoke afterwards binds to this one. Picking the directory
        /// by process architecture keeps x64, x86 and arm64 hosts all working.
        /// </remarks>
        static void PreloadSqliteEngine()
        {
            try
            {
                var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
                var candidate = Path.Combine(Directory, "runtimes", "win-" + architecture, "native", "e_sqlite3.dll");

                //Fully qualified: this namespace also contains SecretNest.ImageStore.File,
                //which otherwise wins over System.IO.File.
                if (System.IO.File.Exists(candidate))
                    NativeLibrary.Load(candidate);
            }
            catch (Exception)
            {
                //Not fatal by itself: the engine may already sit beside the module,
                //where the ordinary P/Invoke search finds it. Failing the import here
                //would replace a working setup with an error, so let any real problem
                //surface from the first database operation instead.
            }
        }
    }
}
