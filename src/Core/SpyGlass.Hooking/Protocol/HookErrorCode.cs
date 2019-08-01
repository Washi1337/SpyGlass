namespace SpyGlass.Hooking.Protocol
{
    public enum HookErrorCode : uint
    {
        Success = 0,

        HookCreationFailed = 0x00000001,
        HookAlreadySet = 0x00000002,
        HookNotSet = 0x00000003,
        HookUnsetFailed = 0x00000004,

        HookEventIdInvalid = 0x00000010,
        HookEventSignalFailed = 0x00000011
    }
}