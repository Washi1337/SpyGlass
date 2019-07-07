// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "Server.h"
#include <iostream>
#include <vector>
#include "Hook.h"
#include "dllmain.h"

#if _DEBUG
#define LOG(e) std::cout << e << std::endl
#else
#define LOG(e)
#endif

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

Hook* _hook;
ConnectedClient* _currentClient;

void _stdcall MyHookCallBack(SIZE_T* stack, SIZE_T* registers)
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

    CallBackMessage message(0, registers[REGISTER_EIP]);
    _currentClient->Send(&message.Header);

#if _DEBUG
    LOG("---- [End Hook Callback] ----");
#endif
}

DWORD WINAPI ActualMain(LPVOID lpParam)
{
    LOG("Hook callback address: " << std::hex << MyHookCallBack);

    Server::InitializeWinSock();

    Server server;
    server.Bind(12345);
    server.Listen();

    while (true)
    {
        _currentClient = server.Accept();        
        while (true)
        {
            MessageHeader* message = _currentClient->Receive();
            LOG("Message received (length: " << message->PayloadLength << ", id: " << message->MessageId << ")");

            switch (message->MessageId)
            {
            case MESSAGE_ID_SETHOOK:
                HandleSetHookMessage(message, _currentClient);
                break;
            }

            delete message;
        }

        delete _currentClient;
    }

    return 0;
}

void HandleSetHookMessage(MessageHeader* message, ConnectedClient* client)
{
    auto setHook = (SetHookMessage*) message;
    LOG(setHook->ToString());

    UINT16* rawOffsets = (UINT16*)((char*) message + sizeof(SetHookMessage));
    std::vector<int> offsets;
    for (int i = 0; i < setHook->FixupCount; i++)
        offsets.push_back(rawOffsets[i]);

    HookParameters parameters = { (void*)setHook->Address, setHook->Count, offsets };

    for (int i = 0; i < parameters.OffsetsNeedingFixup.size(); i++)
        LOG(parameters.OffsetsNeedingFixup[i]);
       
    auto response = ActionCompletedMessage(0);
    try
    {
        _hook = new Hook(parameters, MyHookCallBack);
        _hook->Set();
    }
    catch (int e)
    {
        response.ErrorCode = e;
    }

    client->Send(&response.Header);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:    

        DWORD threadId;
        if (!CreateThread(NULL, 0, ActualMain, NULL, 0, &threadId))
            MessageBoxA(NULL, "Failed to create thread!", "Error", 16);
        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

