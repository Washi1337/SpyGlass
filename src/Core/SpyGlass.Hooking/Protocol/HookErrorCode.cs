namespace SpyGlass.Hooking.Protocol
{
    public enum HookErrorCode : uint
    {
        Success = 0,
        
        HookCreationFailed = 0x00000001,
        HookAlreadySet = 0x00000002
    }
}