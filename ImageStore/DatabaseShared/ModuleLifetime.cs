using System;
using System.Management.Automation;

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

        public void OnImport()
        {
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
    }
}
