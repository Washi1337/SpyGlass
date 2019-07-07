// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "HookSession.h"
#include "dllmain.h"

DWORD WINAPI Bootstrapper(LPVOID lpParam)
{
    Server::InitializeWinSock();

    HookSessionInstance = new HookSession(12345);
    HookSessionInstance->RunMessageLoop();

    return 0;
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
        if (!CreateThread(NULL, 0, Bootstrapper, NULL, 0, &threadId))
        {
            MessageBoxA(NULL, "Failed to create thread!", "Error", 16);
            return false;
        }

        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

