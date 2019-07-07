#pragma once
#include "Server.h"
#include "Hook.h"
#include <map>

#define ERROR_HOOK_CREATION_FAILED 0x00000001
#define ERROR_HOOK_ALREADY_SET     0x00000002

class HookSession
{
public:
    HookSession(int port);
    ~HookSession();

    void RunMessageLoop();
    void HookCallback(SIZE_T* stack, SIZE_T* registers);

private:
    void HandleSetHookMessage(MessageHeader* message);

    Server _server;
    ConnectedClient* _currentClient = nullptr;
    std::map<UINT64, Hook*> _currentHooks;
};

extern HookSession* HookSessionInstance;

