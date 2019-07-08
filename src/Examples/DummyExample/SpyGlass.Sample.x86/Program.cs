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
        private static HookSession _hookSession;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Sample.exe host port");
                return;
            }
            
            string host = args[0];
            int port = int.Parse(args[1]);
            
            Console.WriteLine("Connecting to remote thread...");
            
            _hookSession = new HookSession(new AsmResolverParametersDetector());
            _hookSession.MessageReceived += HookSessionOnMessageReceived;
            _hookSession.MessageSent += HookSessionOnMessageSent;
            _hookSession.HookTriggered += HookSessionOnHookTriggered;
            _hookSession.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            
            Console.Write("Enter address to hook: ");    
            var address = new IntPtr(long.Parse(Console.ReadLine(), NumberStyles.HexNumber));
            
            _hookSession.Set(address);

            Console.WriteLine("Hook set!");
            

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
            Console.WriteLine("--- [Hook callback] ---");
            Console.WriteLine("[Registers]");
            for (int i = 0; i < 9; i++)
            {
                var register = (RegisterX86) i;
                Console.WriteLine("{0}: {1:X8}", register.ToString().ToLowerInvariant(), e.Registers[i]);
            }
            
            var esp = (IntPtr) e.Registers[(int) RegisterX86.Esp];

            Console.WriteLine("[Stack]");
            var data = _hookSession.ReadMemory(esp, 4 * sizeof(int));
            for (int i = 0; i < 4 * sizeof(int); i += sizeof(int))
                Console.WriteLine($"esp+{i:00}: {BitConverter.ToUInt32(data, i):X8}");

            Console.WriteLine("Changing esp+4 to 0x1234");
            _hookSession.WriteMemory(esp + 4, BitConverter.GetBytes(0x1234));

            Console.WriteLine("--- [End hook callback] ---");
        }
    }
}