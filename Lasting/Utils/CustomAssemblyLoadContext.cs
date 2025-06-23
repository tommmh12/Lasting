using System.Reflection;
using System.Runtime.Loader;

namespace Lasting.Utils
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDllFromPath(absolutePath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
