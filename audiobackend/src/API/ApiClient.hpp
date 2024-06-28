//
// Created by Ewen TALLEUX on 28/06/2024.
//

#pragma once

#include <iostream>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

class ApiClient {
    public:
        ApiClient();
        ~ApiClient() = default;

        void run();

    private:
        int _socket;
        struct sockaddr_in _serverAddress{};
};
