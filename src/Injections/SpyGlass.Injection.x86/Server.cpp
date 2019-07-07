
#include "pch.h"
#include "Server.h"
#include <winsock2.h>
#include <ws2tcpip.h>

#define PORT "12345"

void Server::InitializeWinSock()
{
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0)
        throw SERVER_ERROR_WSA_FAILED;
}

Server::Server()
{
    // Get address info.
    struct addrinfo hints;

    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    struct addrinfo* resultInfo = NULL;
    int result = getaddrinfo(NULL, PORT, &hints, &resultInfo);
    if (result != 0)
        throw SERVER_ERROR_GETADDRINFO_FAILED;

    // Create socket
    auto listenSocket = socket(resultInfo->ai_family, resultInfo->ai_socktype, resultInfo->ai_protocol);
    if (listenSocket == INVALID_SOCKET)
    {
        freeaddrinfo(resultInfo);
        throw SERVER_ERROR_SOCKET_CREATION_FAILED;
    }

    this->_listenSocket = listenSocket;
    this->_addressInfo = resultInfo;
}

Server::~Server()
{
}

void Server::Bind(int port)
{
    int result = bind(this->_listenSocket, this->_addressInfo->ai_addr, (int)this->_addressInfo->ai_addrlen);
    if (result == SOCKET_ERROR)
        throw SERVER_ERROR_SOCKET_BIND_FAILED;
}

void Server::Listen()
{
    if (listen(this->_listenSocket, SOMAXCONN) == SOCKET_ERROR)
        throw WSAGetLastError();
}

ConnectedClient* Server::Accept()
{
    auto clientSocket = INVALID_SOCKET;

    // Accept a client socket.
    clientSocket = accept(this->_listenSocket, NULL, NULL);
    if (clientSocket == INVALID_SOCKET)
        throw WSAGetLastError();

    return new ConnectedClient(clientSocket);
}

void Server::Close()
{
    if (this->_addressInfo != NULL)
        freeaddrinfo(this->_addressInfo);
    if (this->_listenSocket != INVALID_SOCKET)
        closesocket(this->_listenSocket);
}
