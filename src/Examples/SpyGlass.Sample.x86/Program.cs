using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using SpyGlass.Hooking;
using SpyGlass.Hooking.Protocol;
using SpyGlass.Injection;

namespace SpyGlass.Sample.x86
{
    class Program
    {
        private static readonly IList<string> RegisterNames = new[]
        {
            "eax",
            "ecx",
            "edx",
            "ebx",
            "esp",
            "ebp",
            "esi",
            "edi",
            "eip",    
        };
        
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
                hookSession.MessageReceived += HookSessionOnMessageReceived;
                hookSession.MessageSent += HookSessionOnMessageSent;
                hookSession.HookTriggered += HookSessionOnHookTriggered;
                hookSession.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
                
                Console.Write("Enter address to hook: ");
                var address = new IntPtr(long.Parse(Console.ReadLine(), NumberStyles.HexNumber));
                
                hookSession.Set(address);

                Console.WriteLine("Hook set!");
            }

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void HookSessionOnMessageSent(object sender, Message e)
        {
            Console.WriteLine("--> " + e);
        }

        private static void HookSessionOnMessageReceived(object sender, Message e)
        {
            Console.WriteLine("<-- " + e);
        }

        private static void HookSessionOnHookTriggered(object sender, HookEventArgs e)
        {
            Console.WriteLine("--- Hook triggered! ---");
            
            Console.WriteLine("[Registers]");
            for (int i = 0; i < RegisterNames.Count; i++)
                Console.WriteLine($"{RegisterNames[i]}: {e.Registers[i]:X8}");
            
            Console.WriteLine("Press a key to continue!");
            Console.ReadKey();
            Console.WriteLine("Continuing!");
        }
    }
}