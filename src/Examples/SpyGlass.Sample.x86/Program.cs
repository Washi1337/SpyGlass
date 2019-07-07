using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using SpyGlass.Hooking;
using SpyGlass.Injection;

namespace SpyGlass.Sample.x86
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Specify base directory.");
                return;
            }

            string baseDirectory = args[0].Replace("\"", "");
            string targetAppPath = Path.Combine(baseDirectory, "SpyGlass.DummyTarget.exe");
            string dllPath = Path.Combine(baseDirectory, "SpyGlass.Injection.x86.dll");

            int id;
            using (var process = Process.Start(targetAppPath))
                id = process.Id;

            using (var remoteProcess = new RemoteProcess(id))
            {
                Console.WriteLine("Injecting spyglass...");
                var injector = new LoadLibraryInjector();
                injector.InjectDll(remoteProcess, dllPath);

                Console.WriteLine("Connecting to remote thread...");
                var hookSession = new HookSession(remoteProcess, new AsmResolverParametersDetector());
                hookSession.HookTriggered += HookSessionOnHookTriggered;
                hookSession.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
                
                Console.Write("Enter address to hook: ");
                var address = new IntPtr(long.Parse(Console.ReadLine(), NumberStyles.HexNumber));
                
                hookSession.Set(address);

                Console.WriteLine("Hook set!");
            }

            Console.ReadKey();
        }

        private static void HookSessionOnHookTriggered(object sender, HookEventArgs e)
        {
            Console.WriteLine($"Hook at address {e.Address.ToInt64():X8} triggered.");
        }
    }
}