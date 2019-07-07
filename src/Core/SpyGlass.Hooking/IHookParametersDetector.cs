using System;

namespace SpyGlass.Hooking
{
    public interface IHookParametersDetector
    {
        HookParameters Detect(HookSession session, IntPtr address);
    }
}