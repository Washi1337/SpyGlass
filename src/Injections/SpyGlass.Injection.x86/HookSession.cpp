#include "pch.h"
#include "HookSession.h"
#include <iostream>

#if _DEBUG
#define LOG(e) std::cout << e << std::endl
#else
#define LOG(e)
#endif

HookSession* HookSessionInstance;

void _stdcall HookCallbackBootstrapper(SIZE_T* stack, SIZE_T* registers)
{
    LOG("--- Entering hook " << std::hex << registers[REGISTER_EIP] << " ---");
    HookSessionInstance->HookCallback(stack, registers);
    LOG("--- Exiting hook " << std::hex << registers[REGISTER_EIP] << " ---");
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
                HandleSetHookMessage((SetHookMessage*) message);
                break;
            case MESSAGE_ID_CONTINUE:
                HandleContinueMessage((ContinueMessage*) message);
                break;
            }

            delete message;
        }

        delete _currentClient;
    }
}

void HookSession::HookCallback(SIZE_T* stack, SIZE_T* registers)
{
    HANDLE waitHandle = CreateEvent(NULL, true, false, NULL);
    if (waitHandle == NULL)
    {
        LOG("ERROR creating wait handle! " << GetLastError() << std::endl);
        return;
    }

    HookEvent e;
    e.WaitEvent = waitHandle;
    e.Stack = stack;
    e.Registers = registers;

    int id = RegisterEvent(e);

    CallBackMessage message(id, registers[REGISTER_EIP]);
    _currentClient->Send(&message.Header);

    LOG("Waiting for continue signal...");

    if (WaitForSingleObjectEx(waitHandle, INFINITE, false) != 0)
    {
        LOG("ERROR wait failed! " << GetLastError() << std::endl);
        return;
    }

    DestroyEvent(id);
    CloseHandle(waitHandle);
}

int HookSession::RegisterEvent(HookEvent e)
{
    int id = _eventCounter++;
    _currentEvents[id] = e;
    return id;
}

void HookSession::DestroyEvent(int id)
{
    _currentEvents.erase(id);
}

void HookSession::HandleSetHookMessage(SetHookMessage* message)
{
    auto response = ActionCompletedMessage(0);
    LOG(message->ToString());
    
    if (_currentHooks.count(message->Address) > 0)
    {
        response.ErrorCode = ERROR_HOOK_ALREADY_SET;
    }
    else 
    {
        // Get fixups
        UINT16* rawOffsets = (UINT16*)((char*)message + sizeof(SetHookMessage));
        std::vector<int> offsets;
        for (int i = 0; i < message->FixupCount; i++)
            offsets.push_back(rawOffsets[i]);

        // Set up hook parameters
        HookParameters parameters;
        parameters.Address = (void*)message->Address;
        parameters.BytesToOverwrite = message->Count;
        parameters.OffsetsNeedingFixup = offsets;

        // Set hook.
        try
        {
            auto hook = new Hook(parameters, HookCallbackBootstrapper);
            _currentHooks[message->Address] = hook;
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

void HookSession::HandleContinueMessage(ContinueMessage* message)
{
    auto response = ActionCompletedMessage(0);
    LOG(message->ToString());

    if (_currentEvents.count(message->Id) == 0) 
    {
        response.ErrorCode = ERROR_HOOK_EVENT_ID_INVALID;
    }
    else
    {
        // Signal hook callback to continue.
        HookEvent e = _currentEvents[message->Id];
        if (SetEvent(e.WaitEvent) == 0)
        {
            response.ErrorCode = ERROR_HOOK_EVENT_SIGNAL_FAILED;
            response.Metadata = GetLastError();
        }
    }

    // Send result back to master process.
    _currentClient->Send(&response.Header);
}