#pragma once
#pragma pack(push, 1)

#include <Windows.h>
#include <string>
#include <sstream>

#define MESSAGE_ID_ACTION_COMPLETED         1
#define MESSAGE_ID_SETHOOK                  2
#define MESSAGE_ID_CALLBACK                 3
#define MESSAGE_ID_CONTINUE                 4
#define MESSAGE_ID_MEM_READ_REQUEST         5
#define MESSAGE_ID_MEM_READ_RESPONSE        6
#define MESSAGE_ID_MEM_EDIT                 7
#define MESSAGE_ID_PROC_ADDRESS_REQUEST     8
#define MESSAGE_ID_PROC_ADDRESS_RESPONSE    9

struct MessageHeader
{
    UINT32 PayloadLength;
    UINT32 MessageId;
    UINT32 SequenceNumber;
};

struct ActionCompletedMessage
{
    MessageHeader Header;
    UINT32 ErrorCode;
    UINT32 Metadata;

    ActionCompletedMessage(UINT32 errorCode)
    {
        Header.PayloadLength = sizeof(ActionCompletedMessage) - sizeof(MessageHeader);
        Header.MessageId = MESSAGE_ID_ACTION_COMPLETED;
        ErrorCode = errorCode;
        Metadata = 0;
    }

    std::string ToString()
    {
        std::stringstream result;
        result << "ActionCompleted(Code: " << ErrorCode << ", Metadata: " << Metadata << ")";
        return result.str();
    }
};

struct SetHookMessage
{
    MessageHeader Header;
    UINT64 Address;
    UINT32 Count;    
    UINT16 FixupCount;

    std::string ToString()
    {
        std::stringstream result;
        result << "SetHook(Address: " << std::hex << Address << ", Count: " << Count << ", Fixups: " << FixupCount << ")";
        return result.str();
    }
};

struct CallBackMessage
{
    MessageHeader Header;
    UINT64 Id;
    UINT32 RegisterCount;

    CallBackMessage(UINT64 id)
    {
        Header.PayloadLength = sizeof(CallBackMessage) - sizeof(MessageHeader);
        Header.MessageId = MESSAGE_ID_CALLBACK;
        Id = id;
        RegisterCount = 0;
    }

    std::string ToString()
    {
        std::stringstream result;
        result << "CallBack(Id: " << Id << ")";
        return result.str();
    }
};

struct ContinueMessage
{
    MessageHeader Header;
    UINT64 Id;
    UINT32 RegisterChangesCount;

    std::string ToString()
    {
        std::stringstream result;
        result << "Continue(Id: " << Id << ", RegisterChanges: " << RegisterChangesCount << ")";
        return result.str();
    }
};

struct RegisterChange 
{
    UINT32 Index;
    UINT64 NewValue;
};

struct MemoryReadRequest
{
    MessageHeader Header;
    UINT64 Address;
    UINT32 Length;

    std::string ToString()
    {
        std::stringstream result;
        result << "MemoryRead(Address: " << std::hex << Address << ", Length: " << Length << ")";
        return result.str();
    }
};

struct MemoryReadResponse
{
    MessageHeader Header;

    MemoryReadResponse()
    {
        Header.MessageId = MESSAGE_ID_MEM_READ_RESPONSE;
    }
};

struct MemoryEditRequest
{
    MessageHeader Header;
    UINT64 Address;

    std::string ToString()
    {
        std::stringstream result;
        result << "MemoryEdit(Address: " << std::hex << Address << ")";
        return result.str();
    }
};

struct ProcAddressRequest
{
    MessageHeader Header;
    UINT16 LibraryNameLength;
    UINT16 ProcedureNameLength;

    std::string ToString()
    {
        std::stringstream result;
        result << "GetProcAddress(Library: " << std::dec << LibraryNameLength << ", Procedure: " << ProcedureNameLength << ")";
        return result.str();
    }
};

struct ProcAddressResponse
{
    MessageHeader Header;
    UINT64 Address;

    ProcAddressResponse(UINT64 address)
    {
        Header.PayloadLength = sizeof(ProcAddressResponse) - sizeof(MessageHeader);
        Header.MessageId = MESSAGE_ID_PROC_ADDRESS_RESPONSE;
        Address = address;
    }
};

#pragma pack(pop)