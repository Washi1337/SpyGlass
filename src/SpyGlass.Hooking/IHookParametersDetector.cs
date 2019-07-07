using System;

namespace SpyGlass.Hooking
{
    public interface IHookParametersDetector
    {
        HookParameters Detect(RemoteProcess process, IntPtr address);
    }
}