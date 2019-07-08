using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using SpyGlass.Hooking;
using SpyGlass.Sample.x86;

namespace MessageBoxHook
{
    internal class Program
    {
        private static HookSession _hookSession;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: MessageBoxHook.exe host port");
                return;
            }
            
            string host = args[0];
            int port = int.Parse(args[1]);
            
            Console.WriteLine("Connecting to remote thread...");
            
            _hookSession = new HookSession(new AsmResolverParametersDetector());
            _hookSession.HookTriggered += HookSessionOnHookTriggered;
            _hookSession.Connect(new IPEndPoint(IPAddress.Parse(host), port));

            Console.WriteLine("Hooking MessageBoxA...");
            var address = _hookSession.GetProcAddress("user32.dll", "MessageBoxA");
            _hookSession.Set(address);
            Console.WriteLine("Hook set!");
            
            Process.GetCurrentProcess().WaitForExit();
        }

        private static string BytesToZeroTerminatedString(byte[] data)
        {
            int zeroIndex = Array.IndexOf(data, (byte) 0);
            if (zeroIndex == -1)
                zeroIndex = data.Length;
            return Encoding.ASCII.GetString(data, 0, zeroIndex);
        }

        private static void HookSessionOnHookTriggered(object sender, HookEventArgs e)
        {
            Console.WriteLine("MessageBoxA was called!");

            var esp = (IntPtr) e.Registers[(int) RegisterX86.Esp];
            var rawStackData = _hookSession.ReadMemory(esp, 5 * sizeof(uint));
            
            var stackEntries = new uint[5];
            for (int i = 0; i < stackEntries.Length; i ++)
                stackEntries[i] = BitConverter.ToUInt32(rawStackData, i*sizeof(int));

            var message = BytesToZeroTerminatedString(_hookSession.ReadMemory((IntPtr) stackEntries[2], 100));
            var title = BytesToZeroTerminatedString(_hookSession.ReadMemory((IntPtr) stackEntries[3], 100));
            
            Console.WriteLine("Arguments:");
            Console.WriteLine($"- hWnd: {stackEntries[1]:X8}");
            Console.WriteLine($"- lpText: \"{message}\"");
            Console.WriteLine($"- lpCaption: \"{title}\"");
            Console.WriteLine($"- uType: {stackEntries[4]:X8}");
        }
    }
}