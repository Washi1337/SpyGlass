#include "pch.h"
#include "HookSession.h"
#include <iostream>

#if _DEBUG
#define LOG(e) std::cout << e << std::endl
#else
#define LOG(e)
#endif

HookSession* HookSessionInstance;

const char _registerNames[8][4] =
{
    "edi",
    "esi",
    "ebp",
    "ebx",
    "edx",
    "ecx",
    "eax",
    "eip"
};

void _stdcall HookCallbackBootstrapper(SIZE_T* stack, SIZE_T* registers)
{
#if _DEBUG
    LOG("---- [Begin Hook Callback] ----");

    LOG("---- [Registers] ----");
    for (int i = 7; i >= 0; i--)
        LOG(_registerNames[i] << ": " << std::hex << registers[i]);

    LOG("---- [First 4 stack values] ----");
    for (int i = 0; i < 4; i++)
        LOG("esp+" << (i * sizeof(SIZE_T)) << ": " << std::hex << stack[i]);
#endif

    HookSessionInstance->HookCallback(stack, registers);

#if _DEBUG
    LOG("---- [End Hook Callback] ----");
#endif
}

HookSession::HookSession(int port)
    : _server(port)
{
    _server.Bind();
    _server.Listen();
}

HookSession::~HookSession()
{
    for (auto entry : _currentHooks)
          delete entry.second;
}

void HookSession::RunMessageLoop()
{
    while (true)
    {
        _currentClient = _server.Accept();

        while (true)
        {
            MessageHeader* message = _currentClient->Receive();
            LOG("Message received (length: " << message->PayloadLength << ", id: " << message->MessageId << ")");

            switch (message->MessageId)
            {
            case MESSAGE_ID_SETHOOK:
                HandleSetHookMessage(message);
                break;
            }

            delete message;
        }

        delete _currentClient;
    }
}

void HookSession::HookCallback(SIZE_T* stack, SIZE_T* registers)
{
    CallBackMessage message(0, registers[REGISTER_EIP]);
    _currentClient->Send(&message.Header);
}

void HookSession::HandleSetHookMessage(MessageHeader* message)
{
    auto response = ActionCompletedMessage(0);

    auto setHook = (SetHookMessage*)message;
    LOG(setHook->ToString());
    
    if (_currentHooks.count(setHook->Address) > 0)
    {
        response.ErrorCode = ERROR_HOOK_ALREADY_SET;
    }
    else 
    {
        // Get fixups
        UINT16* rawOffsets = (UINT16*)((char*)message + sizeof(SetHookMessage));
        std::vector<int> offsets;
        for (int i = 0; i < setHook->FixupCount; i++)
            offsets.push_back(rawOffsets[i]);

        // Set up hook parameters
        HookParameters parameters;
        parameters.Address = (void*)setHook->Address;
        parameters.BytesToOverwrite = setHook->Count;
        parameters.OffsetsNeedingFixup = offsets;

        // Set hook.
        try
        {
            auto hook = new Hook(parameters, HookCallbackBootstrapper);
            _currentHooks[setHook->Address] = hook;
            hook->Set();
        }
        catch (int e)
        {
            response.ErrorCode = ERROR_HOOK_CREATION_FAILED;
            response.Metadata = e;
        }
    }

    // Send result back to master process.
    _currentClient->Send(&response.Header);
}
