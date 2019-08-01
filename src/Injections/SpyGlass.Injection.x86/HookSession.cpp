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
    LOG("--- [Entering hook " << std::hex << registers[REGISTER_EIP] << "] ---");
    HookSessionInstance->HookCallback(stack, registers);
    LOG("--- [Exiting hook " << std::hex << registers[REGISTER_EIP] << "] ---");
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
            // Fetch
            auto message = _currentClient->Receive();
            LOG("Message received (length: " << message->PayloadLength << ", id: " << message->MessageId << ", seq: " << message->SequenceNumber << ")");

            // Dispatch
            switch (message->MessageId)
            {
            case MESSAGE_ID_SETHOOK:
                HandleSetHookMessage((SetHookMessage*) message);
                break;
            case MESSAGE_ID_UNSETHOOK:
                HandleUnsetHookMessage((UnsetHookMessage*) message);
                break;
            case MESSAGE_ID_CONTINUE:
                HandleContinueMessage((ContinueMessage*) message);
                break;
            case MESSAGE_ID_MEM_READ_REQUEST:
                HandleMemoryReadRequest((MemoryReadRequest*)message);
                break;
            case MESSAGE_ID_MEM_EDIT:
                HandleMemoryEditRequest((MemoryEditRequest*) message);
                break;
            case MESSAGE_ID_PROC_ADDRESS_REQUEST:
                HandleProcAddressRequest((ProcAddressRequest*)message);
                break;
            default:
                LOG("Unrecognized message.");
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

    int size = sizeof(CallBackMessage) + sizeof(UINT64) * REGISTER_COUNT;
        
    char* response = new char[size];
    auto message = (CallBackMessage*) response;
    auto regs = (UINT64*) (response + sizeof(CallBackMessage));

    message->Header.MessageId = MESSAGE_ID_CALLBACK;
    message->Header.PayloadLength = size - sizeof(MessageHeader);
    message->Id = id;
    message->RegisterCount = REGISTER_COUNT;

    for (int i = 0; i < REGISTER_COUNT; i++)
        regs[i] = registers[i];

    _currentClient->Send(&message->Header);
    delete response;

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
    response.Header.SequenceNumber = message->Header.SequenceNumber;
    _currentClient->Send(&response.Header);
}

void HookSession::HandleUnsetHookMessage(UnsetHookMessage* message)
{
    auto response = ActionCompletedMessage(0);
    LOG(message->ToString());

    if (_currentHooks.count(message->Address) == 0)
    {
        // No hook was set on this address.
        response.ErrorCode = ERROR_HOOK_NOT_SET;
    }
    else 
    {
        try
        {
            // Unset hook and remove.
            auto hook = _currentHooks[message->Address];
            hook->Unset();
            _currentHooks.erase(message->Address);
        } 
        catch (int e)
        {
            response.ErrorCode = ERROR_HOOK_UNSET_FAILED;
            response.Metadata = e;
        }
    }    
    
    // Send result back to master process.
    response.Header.SequenceNumber = message->Header.SequenceNumber;
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
        HookEvent e = _currentEvents[message->Id];
        
        // Apply requested changes to registers.
        auto changes = (RegisterChange*)((char*) message + sizeof(ContinueMessage));
        for (int i = 0; i < message->RegisterChangesCount; i++)
            e.Registers[changes[i].Index] = changes[i].NewValue & MAXSIZE_T;

        // Signal hook callback to continue.
        if (SetEvent(e.WaitEvent) == 0)
        {
            response.ErrorCode = ERROR_HOOK_EVENT_SIGNAL_FAILED;
            response.Metadata = GetLastError();
        }
    }

    // Send result back to master process.
    response.Header.SequenceNumber = message->Header.SequenceNumber;
    _currentClient->Send(&response.Header);
}

void HookSession::HandleMemoryReadRequest(MemoryReadRequest* message)
{
    auto rawData = new char[sizeof(MemoryReadResponse) + message->Length];
    auto response = (MemoryReadResponse*)rawData;

    response->Header.MessageId = MESSAGE_ID_MEM_READ_RESPONSE;
    response->Header.PayloadLength = message->Length;
    response->Header.SequenceNumber = message->Header.SequenceNumber;

    memcpy(rawData + sizeof(MemoryReadResponse), (void*)message->Address, message->Length);

    _currentClient->Send(&response->Header);
    delete rawData;
}

void HookSession::HandleMemoryEditRequest(MemoryEditRequest* message)
{
    auto response = ActionCompletedMessage(0);
    LOG(message->ToString());

    char* data = (char*) message + sizeof(MemoryEditRequest);

    int length = message->Header.PayloadLength - sizeof(UINT64);
    LOG(std::dec << length);
    memcpy((void*) message->Address, data, length);

    // Send result back to master process.
    response.Header.SequenceNumber = message->Header.SequenceNumber;
    _currentClient->Send(&response.Header);
}

void HookSession::HandleProcAddressRequest(ProcAddressRequest* message)
{
    LOG(message->ToString());

    auto raw = (char*) message + sizeof(ProcAddressRequest);

    // Read raw names.
    auto library = new char[message->LibraryNameLength + 1];
    auto procedure = new char[message->ProcedureNameLength + 1];
    memset(library, 0, message->LibraryNameLength + 1);
    memset(procedure, 0, message->ProcedureNameLength + 1);
    memcpy(library, raw, message->LibraryNameLength);
    memcpy(procedure, raw + message->LibraryNameLength, message->ProcedureNameLength);
    
    // Get requested address.
    auto moduleHandle = GetModuleHandleA(library);
    auto procAddress = GetProcAddress(moduleHandle, procedure);

    LOG(std::hex << procAddress);

    // Clear up buffers.
    delete library;
    delete procedure;

    // Send response.
    auto response = ProcAddressResponse((UINT64) procAddress);
    response.Header.SequenceNumber = message->Header.SequenceNumber;
    _currentClient->Send(&response.Header);
}
