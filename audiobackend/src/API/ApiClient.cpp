//
// Created by Ewen TALLEUX on 28/06/2024.
//

#include "ApiClient.hpp"

ApiClient::ApiClient()
{
    _socket = socket(AF_INET, SOCK_STREAM, 0);
    if (_socket == -1) {
        std::cerr << "Error creating socket" << std::endl;
        exit(1);
    }

    _serverAddress.sin_family = AF_INET;
    _serverAddress.sin_port = htons(6942);
    _serverAddress.sin_addr.s_addr = inet_addr("127.0.0.1");

    if (connect(_socket, (struct sockaddr *)&_serverAddress, sizeof(_serverAddress)) == -1) {
        std::cerr << "Error connecting to server" << std::endl;
        exit(1);
    }
}

void ApiClient::run()
{
    char buffer[1024] = {0};
    while (true) {
        memset(buffer, 0, sizeof(buffer));
        recv(_socket, buffer, 1024, 0);
        std::cout << buffer << std::endl;
    }
}
