using System.Text;
using SpyGlass.Interop;

namespace SpyGlass.Injection
{
    public class LoadLibraryInjector : IInjector
    {
        public void InjectDll(RemoteProcess process, string pathToDll)
        {
            // Write path to dll into the process.
            var pathLocation = process.Allocate(256, MemoryProtection.ReadWrite);
            process.WriteMemory(pathLocation, Encoding.ASCII.GetBytes(pathToDll));

            // Obtain address of LoadLibraryA
            var loadLibraryAddress = Kernel32.GetProcAddress(
                Kernel32.GetModuleHandle("kernel32"), 
                "LoadLibraryA");

            // Create remote thread.
            using (var thread = process.CreateThread(loadLibraryAddress, pathLocation))
            {
                // ...
            }
        }
    }
}