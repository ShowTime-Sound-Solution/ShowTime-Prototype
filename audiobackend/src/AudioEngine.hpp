//
// Created by Ewen TALLEUX on 27/06/2024.
//

#pragma once

#include <memory>
#include "RtAudio.h"
#include "Effects/GainEffect.hpp"

class ApiClient;

class AudioEngine {
    public:
        AudioEngine();
        ~AudioEngine();

        void start();
        void stop();

        void changeOutputDevice(int deviceId);

        std::vector<std::unique_ptr<IEffect>> &getEffects();

        char *getOutputDevicesAvailable();

        void setApiClient(ApiClient *apiClient) {
            _apiClient = apiClient;
        }

        ApiClient *getApiClient() {
            return _apiClient;
        }

    private:
        RtAudio adc = RtAudio(RtAudio::Api::MACOSX_CORE);
        RtAudio::StreamParameters inputParams;
        RtAudio::StreamParameters outputParams;
        bool already_draw = false;

        unsigned int bufferFrames = 512;
        unsigned int sampleRate = 44100;

        std::vector<std::unique_ptr<IEffect>> effects;

        ApiClient *_apiClient;

        static int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
                         double streamTime, RtAudioStreamStatus status, void *userData);

        void findShowTimeVirtualDevice();

        void loadEffects();

        void processEffects(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames);

};
