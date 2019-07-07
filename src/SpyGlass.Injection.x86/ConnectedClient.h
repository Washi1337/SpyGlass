#pragma once
#include <winsock2.h>
#include "Protocol.h"

class ConnectedClient
{
public:
	ConnectedClient(SOCKET clientSocket);
	~ConnectedClient();

	int Send(MessageHeader* message);
	MessageHeader* Receive();

	void Close();

private:
	SOCKET _clientSocket;

};

