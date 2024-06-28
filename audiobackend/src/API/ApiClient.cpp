//
// Created by Ewen TALLEUX on 28/06/2024.
//

#include "ApiClient.hpp"

ApiClient::ApiClient(std::shared_ptr<AudioEngine> audioEngine) : _audioEngine(std::move(audioEngine))
{
    _socket = socket(AF_INET, SOCK_STREAM, 0);
    if (_socket == -1) {
        std::cerr << "Error creating socket" << std::endl;
        exit(1);
    }

    _serverAddress.sin_family = AF_INET;
    _serverAddress.sin_port = htons(6942);
    _serverAddress.sin_addr.s_addr = inet_addr("10.17.72.153");

    if (connect(_socket, (struct sockaddr *)&_serverAddress, sizeof(_serverAddress)) == -1) {
        std::cerr << "Error connecting to server" << std::endl;
        exit(1);
    }
}

void ApiClient::run()
{
    char buffer[1024] = {0};
    while (_running) {
        memset(buffer, 0, sizeof(buffer));
        recv(_socket, buffer, 1024, 0);
        std::cout << buffer << std::endl;
        if (strcmp(buffer, "stop\n") == 0) {
            stop();
        }
        int num = atoi(buffer);
        if (num > 0) {
            _audioEngine->changeOutputDevice(num);
            continue;
        }
        if (strcmp(buffer, "p\n") == 0) {
            GainEffect *gainEffect = nullptr;
            for (auto &effect : _audioEngine->getEffects()) {
                gainEffect = dynamic_cast<GainEffect *>(effect.get());
            }
            if (gainEffect != nullptr) {
                gainEffect->setGain(gainEffect->getGain() + 0.1f);
                std::cout << "Gain: " << gainEffect->getGain() << std::endl;
            }
            continue;
        }
        if (strcmp(buffer, "m\n") == 0) {
            GainEffect *gainEffect = nullptr;
            for (auto &effect : _audioEngine->getEffects()) {
                gainEffect = dynamic_cast<GainEffect *>(effect.get());
            }
            if (gainEffect != nullptr) {
                gainEffect->setGain(gainEffect->getGain() - 0.1f);
                std::cout << "Gain: " << gainEffect->getGain() << std::endl;
            }
            continue;
        }
        std::cout << "Buffer: \"" << buffer[0] << "\"" << std::endl;
        //check if buffer is "0"
        if (buffer[0] == 0x01) {
            std::cout << "Getting output devices" << std::endl;
            char *devices = _audioEngine->getOutputDevicesAvailable();
            send(devices);
            continue;
        }
    }
}
