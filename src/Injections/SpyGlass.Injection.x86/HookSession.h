#pragma once
#include "Server.h"
#include "Hook.h"
#include <map>
#include <atomic>

struct HookEvent
{
    HANDLE WaitEvent;
    SIZE_T* Stack;
    SIZE_T* Registers;
};

#define ERROR_HOOK_CREATION_FAILED      0x00000001
#define ERROR_HOOK_ALREADY_SET          0x00000002

#define ERROR_HOOK_EVENT_ID_INVALID     0x00000010
#define ERROR_HOOK_EVENT_SIGNAL_FAILED  0x00000011

class HookSession
{
public:
    HookSession(int port);
    ~HookSession();

    void RunMessageLoop();
    void HookCallback(SIZE_T* stack, SIZE_T* registers);
    int RegisterEvent(HookEvent e);
    void DestroyEvent(int id);

private:
    void HandleSetHookMessage(SetHookMessage* message);
    void HandleContinueMessage(ContinueMessage* message);
    void HandleMemoryReadRequest(MemoryReadRequest* message);
    void HandleMemoryEditRequest(MemoryEditRequest* message);
    void HandleProcAddressRequest(ProcAddressRequest* message);

    std::atomic_int _eventCounter;

    Server _server;
    ConnectedClient* _currentClient = nullptr;
    std::map<UINT64, Hook*> _currentHooks;
    std::map<UINT32, HookEvent> _currentEvents;
};

extern HookSession* HookSessionInstance;

