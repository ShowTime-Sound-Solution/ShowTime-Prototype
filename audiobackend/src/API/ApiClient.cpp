//
// Created by Ewen TALLEUX on 28/06/2024.
//

#include "ApiClient.hpp"

#include <sys/stat.h>

ApiClient::ApiClient(std::shared_ptr<AudioEngine> audioEngine) : _audioEngine(std::move(audioEngine))
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
    char *buffer = static_cast<char *>(malloc(1024));
    while (_running) {
        memset(buffer, 0, 1024);
        recv(_socket, buffer, 1024, 0);
        /*int num = atoi(buffer);
        if (num > 0) {
            _audioEngine->changeOutputDevice(num);
            continue;
        }*/
        std::cout << "Buffer : " << buffer << std::endl;
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
        handlingCommand(buffer);
    }
    free(buffer);
}

void ApiClient::handlingCommand(char *buffer)
{
    if (strcmp(buffer, "stop\n") == 0) {
        stop();
    }

    if (buffer[0] == 0x30/*0x01*/) {
        sendOutputDevicesAvailable();
        return;
    }
    if (buffer[0] == 0x31/*0x02*/) {
        int num = atoi(&buffer[1]);
        std::cout << "device requested : " << num << std::endl;
        char *response = new char[2] {0x02, static_cast<char>(num)};
        _audioEngine->changeOutputDevice(num);
        send(response);
        return;
    }
    if (buffer[0] == 0x32/*0x05*/) {
        int idEffect = atoi(&buffer[1]);
        std::cout << "Effect requested : " << idEffect << std::endl;
        for (auto &effect : _audioEngine->getEffects()) {
            if (effect->getId() == idEffect) {
                effect->setEnabled(!effect->isEnabled());
                std::cout << "Effect " << idEffect << " is now " << (effect->isEnabled() ? "enabled" : "disabled") << std::endl;
                char *response = new char[2] {0x05, static_cast<char>(idEffect)};
                send(response);
            }
        }
        return;
    }
    if (buffer[0] == 0x33/*0x06*/) {
        //set gain
        float gain = atof(&buffer[1]);
        for (auto &effect : _audioEngine->getEffects()) {
            GainEffect *gainEffect = dynamic_cast<GainEffect *>(effect.get());
            if (gainEffect != nullptr) {
                gainEffect->setGain(gain);
                std::cout << "Gain set to " << gain << std::endl;
            }
        }
    }
    if (buffer[0] == 0x34/*0x07*/) {
        short selectorReverb = atoi(&buffer[1]);
        for (auto &effect : _audioEngine->getEffects()) {
            ReverbEffect *reverbEffect = dynamic_cast<ReverbEffect *>(effect.get());
            if (reverbEffect != nullptr) {
                reverbEffect->changeSelector(selectorReverb);
                std::cout << "Reverb selector changed to " << selectorReverb << std::endl;
            }
        }
    }
    if (buffer[0] == 0x35/*0x08*/) {
        float selectorPan = atof(&buffer[1]);
        for (auto &effect : _audioEngine->getEffects()) {
            PanEffect *panEffect = dynamic_cast<PanEffect *>(effect.get());
            if (panEffect != nullptr) {
                panEffect->setPan(selectorPan);
                std::cout << "Pan selector changed to " << selectorPan << std::endl;
            }
        }
    }
    if (buffer[0] == 0x36/*0x09*/) {
        float volumeInputValue = atof(&buffer[1]);
        _audioEngine->setVolumeInput(volumeInputValue);
    }
    if (buffer[0] == 0x37/*0x0A*/) {
        float volumeOutputValue = atof(&buffer[1]);
        _audioEngine->setVolumeOutput(volumeOutputValue);
    }
}

void ApiClient::sendOutputDevicesAvailable()
{
    std::string devices = _audioEngine->getOutputDevicesAvailable();
    char *result = new char[devices.size() + 2] {0};
    result[0] = 0x02;
    for (int i = 0; i < devices.size(); i++) {
        result[i + 1] = devices[i];
    }
    std::cout << "Sending output devices available" << std::endl;
    send(result);
}

void ApiClient::sendOutputBuffer(char *buffer) const
{
    char *result = new char[512 * 4 + 2] {0};
    result[0] = 0x03;
    for (int i = 0; i < 512 * 4; i++) {
        result[i + 1] = buffer[i];
    }
    send(result);
    delete[] result;
}

void ApiClient::sendInputBuffer(char *buffer) const
{
    char *result = new char[512 * 4 + 2] {0};
    result[0] = 0x04;
    for (int i = 0; i < 512 * 4; i++) {
        result[i + 1] = buffer[i];
    }
    send(result);
    delete[] result;
}
