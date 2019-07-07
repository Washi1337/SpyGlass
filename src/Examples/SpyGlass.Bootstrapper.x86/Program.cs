using System;
using System.IO;
using System.Linq;
using SpyGlass.Injection;

namespace SpyGlass.Bootstrapper.x86
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Bootstrapper.exe injection-dll target-file");
                return;
            }

            string dllPath = args[0].Replace("\"", "");
            string targetAppPath = args[1].Replace("\"", "");

            if (!File.Exists(targetAppPath))
            {
                Console.WriteLine("Target application does not exist!");
                return;
            }
            
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("Injection DLL does not exist!");
                return;
            }

            using (var process = RemoteProcess.Create(args[1], string.Join(" ", args.Skip(2)), true))
            {
                Console.WriteLine("Created process " + process.Id);

                Console.WriteLine("Injecting " + dllPath);
                var injector = new LoadLibraryInjector();
                injector.InjectDll(process, dllPath);

                Console.WriteLine("Resuming process...");
                process.Resume();

                Console.ReadKey();
            }

        }
    }
}