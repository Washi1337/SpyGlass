#include "pch.h"
#include "Hook.h"
#include <Windows.h>
#include <iostream>

#define TRAMPOLINE_OFFSET_CALL 27

const char TrampolineCode[46] = 
{                                    // (implicit "push eip" from call) ; save all registers
    0x50,                            // push eax
    0x51,                            // push ecx
    0x52,                            // push edx
    0x53,                            // push ebx
    0x55,                            // push ebp
    0x56,                            // push esi
    0x57,                            // push edi
                                    
    0x90,                            // nop
    0x90,                            // nop
    
    0x8B, 0x44, 0x24, 0x1C,          // mov eax, [esp+0x1C]    ; Adjust pushed eip so it is the original address.
    0x83, 0xE8, 0x05,                // sub eax, 5
    0x89, 0x44, 0x24, 0x1C,          // mov [esp+0x1C], eax
    
    0x89, 0xE0,                      // mov eax, esp            ; push ptr to registers
    0x50,                            // push eax
    0x83, 0xC0, 0x20,                // add eax, 0x20           ; push ptr to stack
    0x50,                            // push eax
    0xE8, 0x00, 0x00, 0x00, 0x00,    // call <callback>         ; call callback

    0x90,                            // nop
    0x90,                            // nop

    0x5F,                            // pop edi                 ; restore all registers
    0x5E,                            // pop esi
    0x5D,                            // pop ebp
    0x5B,                            // pop ebx
    0x5A,                            // pop edx
    0x59,                            // pop ecx
    0x58,                            // pop eax

    0x83, 0xC4, 0x04,                // add esp, 4              ; pop off eip

    0x90,                            // nop
    0x90,                            // nop
};

#define TRAMPOLINE_OFFSET_RETURN 2

const char TrampolineCodeEpilogue[7] = {
    0x90,                            // nop
    0x90,                            // nop
    0xE9, 0x00, 0x00, 0x00, 0x00     // jmp <return address>    ; jump instead of normal ret, injected instructions might have altered the stack already.
};

Hook::Hook(HookParameters parameters, HookCallBack callback)
{
    _parameters = parameters;
    _callback = callback;

    ReadBytesToOverwrite();
    CreateTrampoline();
    CreateHookBytes();
}

Hook::~Hook()
{
    if (_isSet)
        Unset();

    delete _originalBytes;
    delete _hookBytes;
    delete _trampoline;
}

void Hook::Set()
{
    if (_isSet)
        return;

    DWORD old;
    if (!VirtualProtect(_parameters.Address, _parameters.BytesToOverwrite, PAGE_EXECUTE_READWRITE, &old))
        throw GetLastError();
    
    // Copy over bytes.
    memcpy(_parameters.Address, _hookBytes, _parameters.BytesToOverwrite);
    VirtualProtect(_parameters.Address, _parameters.BytesToOverwrite, old, &old);

    _isSet = true;
}

void Hook::Unset()
{
    if (!_isSet)
        return;

    DWORD old;
    if (!VirtualProtect(_parameters.Address, _parameters.BytesToOverwrite, PAGE_EXECUTE_READWRITE, &old))
        throw GetLastError();

    // Copy over bytes.
    memcpy(_parameters.Address, _originalBytes, _parameters.BytesToOverwrite);
    VirtualProtect(_parameters.Address, _parameters.BytesToOverwrite, old, &old);

    _isSet = false;
}

void Hook::ReadBytesToOverwrite()
{
    _originalBytes = new char[_parameters.BytesToOverwrite];
    memcpy(_originalBytes, _parameters.Address, _parameters.BytesToOverwrite);
}

void Hook::CreateTrampoline()
{
    // Allocate trampoline.
    SIZE_T sizeOfTrampoline = sizeof(TrampolineCode) + _parameters.BytesToOverwrite + sizeof(TrampolineCodeEpilogue);
    char* trampoline = (char*) VirtualAlloc(
        NULL, 
        sizeOfTrampoline, 
        MEM_COMMIT | MEM_RESERVE, 
        PAGE_EXECUTE_READWRITE);

    if (trampoline == NULL)
        throw GetLastError();
    
    // Copy over trampoline bytes + original bytes.
    memcpy(trampoline, TrampolineCode, sizeof(TrampolineCode));
    memcpy(trampoline + sizeof(TrampolineCode), _originalBytes, _parameters.BytesToOverwrite);
    memcpy(trampoline + sizeof(TrampolineCode) + _parameters.BytesToOverwrite, TrampolineCodeEpilogue, _parameters.BytesToOverwrite);

    // Adjust callback address in trampoline.
    char* pointerToCall = trampoline + TRAMPOLINE_OFFSET_CALL;
    *(DWORD*)(pointerToCall + 1) = (DWORD) _callback - (DWORD) (pointerToCall + 5);

    // Adjust original bytes using the fixup data.
    char* injectedOriginalBytes = trampoline + sizeof(TrampolineCode);
    for (int i = 0; i < _parameters.OffsetsNeedingFixup.size(); i++)
    {
        int offset = _parameters.OffsetsNeedingFixup[i];

        // Get location of original and new operand.
        char* originalAddress = (char*) _parameters.Address + offset;
        char* addressToFix = injectedOriginalBytes + offset;

        // Translate operand.
        DWORD absoluteAddress = (DWORD) originalAddress + (*(DWORD*) originalAddress) + 4;
        DWORD newRelativeAddress = absoluteAddress - (DWORD) (addressToFix + 4);
               
        // Write new operand.
        *(DWORD*)(addressToFix) = newRelativeAddress;
    }

    // Adjust return address in trampoline epilogue.
    char* pointerToReturn = trampoline + sizeof(TrampolineCode) + _parameters.BytesToOverwrite + TRAMPOLINE_OFFSET_RETURN;
    char* returnAddress = (char*) _parameters.Address + _parameters.BytesToOverwrite;
    *(DWORD*)(pointerToReturn + 1) = (DWORD) returnAddress - (DWORD) (pointerToReturn + 5);

    _trampoline = trampoline;
}

void Hook::CreateHookBytes()
{
    _hookBytes = new char[_parameters.BytesToOverwrite];

    // Encode a jmp.
    DWORD relativeOffset = (DWORD)_trampoline - ((DWORD)_parameters.Address + 5);
    _hookBytes[0] = 0xE8;
    *(DWORD*)(_hookBytes + 1) = relativeOffset;

    // Fill the remaining with NOPs.
    for (int i = 5; i < _parameters.BytesToOverwrite; i++)
        _hookBytes[i] = 0x90;
}
