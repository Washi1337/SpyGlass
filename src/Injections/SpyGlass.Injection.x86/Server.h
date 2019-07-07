#pragma once

#include <winsock2.h>
#include "ConnectedClient.h"

#pragma comment(lib, "Ws2_32.lib")

#define SERVER_ERROR_WSA_FAILED 1
#define SERVER_ERROR_GETADDRINFO_FAILED 2
#define SERVER_ERROR_SOCKET_CREATION_FAILED 3
#define SERVER_ERROR_SOCKET_BIND_FAILED 4

class Server
{
public:
    static void InitializeWinSock();

    Server(int port);
    ~Server();

    void Bind();
    void Listen();

    ConnectedClient* Accept();

    void Close();

private:

    SOCKET _listenSocket = INVALID_SOCKET;
    struct addrinfo* _addressInfo = NULL;
};

