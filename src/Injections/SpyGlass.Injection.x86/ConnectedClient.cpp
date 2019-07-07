
#include "pch.h"

#include "ConnectedClient.h"
#include <iostream>

ConnectedClient::ConnectedClient(SOCKET clientSocket)
{
    this->_clientSocket = clientSocket;
}

ConnectedClient::~ConnectedClient()
{
    Close();
}

int ConnectedClient::Send(MessageHeader* message)
{
    int result = send(this->_clientSocket, (char*) message, sizeof(MessageHeader) + message->PayloadLength, 0);

    if (result == SOCKET_ERROR)
        throw WSAGetLastError();
}

MessageHeader* ConnectedClient::Receive()
{
    char header[sizeof(MessageHeader)] = {};

    int result = recv(this->_clientSocket, header, sizeof(MessageHeader), 0);
    if (result == SOCKET_ERROR)
        throw WSAGetLastError();

    int payloadLength = header[0];

    char* buffer = new char[sizeof(MessageHeader) + payloadLength];
    memcpy(buffer, header, sizeof(MessageHeader));
    
    result = recv(this->_clientSocket, buffer + sizeof(MessageHeader), payloadLength, 0);
    if (result == SOCKET_ERROR)
        throw WSAGetLastError();

    auto message = (MessageHeader*)buffer;
    return message;
}

void ConnectedClient::Close()
{
    closesocket(this->_clientSocket);
}
