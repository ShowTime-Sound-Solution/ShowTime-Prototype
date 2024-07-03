//
// Created by Ewen TALLEUX on 28/06/2024.
//

#pragma once

#include <iostream>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <cstring>
#include <unistd.h>
#include "../AudioEngine.hpp"

class ApiClient {
    public:
        ApiClient(std::shared_ptr<AudioEngine> audioEngine);
        ~ApiClient() = default;

        void run();

        void send(const char *data) const {
            write(_socket, data, strlen(data));
        }

        void stop() {
            _running = false;
        }

        bool isRunning() const {
            return _running;
        }

        void handlingCommand(char *buffer);

        void sendOutputBuffer(char *buffer) const;

        void sendInputBuffer(char *buffer) const;

        void sendEffectsAvailable();

        void switchEffect(char *buffer);

    private:
        int _socket;
        struct sockaddr_in _serverAddress{};
        bool _running = true;

        std::shared_ptr<AudioEngine> _audioEngine;

        void sendOutputDevicesAvailable();
};
