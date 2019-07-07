#pragma once
#pragma pack(push, 1)

#include <Windows.h>

#if _DEBUG
#include <string>
#include <sstream>
#endif

#define MESSAGE_ID_ACTION_COMPLETED 1
#define MESSAGE_ID_SETHOOK 2
#define MESSAGE_ID_CALLBACK 3
#define MESSAGE_ID_CONTINUE 4

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

#if _DEBUG
    std::string ToString()
    {
        std::stringstream result;
        result << "ActionCompleted(Code: " << ErrorCode << ", Metadata: " << Metadata << ")";
        return result.str();
    }
#endif

};

struct SetHookMessage
{
    MessageHeader Header;
    UINT64 Address;
    UINT32 Count;    
    UINT16 FixupCount;

#if _DEBUG
    std::string ToString()
    {
        std::stringstream result;
        result << "SetHook(Address: " << std::hex << Address << ", Count: " << Count << ", Fixups: " << FixupCount << ")";
        return result.str();
    }
#endif

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
    }

#if _DEBUG
    std::string ToString()
    {
        std::stringstream result;
        result << "CallBack(Id: " << Id << ")";
        return result.str();
    }
#endif

};

struct ContinueMessage
{
    MessageHeader Header;
    UINT64 Id;

#if _DEBUG
    std::string ToString()
    {
        std::stringstream result;
        result << "Continue(Id: " << Id << ")";
        return result.str();
    }
#endif
};

#pragma pack(pop)