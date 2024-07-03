//
// Created by Ewen TALLEUX on 27/06/2024.
//

#pragma once

#include <memory>
#include "RtAudio.h"
#include "Effects/GainEffect.hpp"
#include "Effects/PhaseInverterEffect.hpp"
#include "Effects/EqualizerEffect.hpp"
#include "Effects/ReverbEffect.hpp"
#include "Effects/PanEffect.hpp"

class ApiClient;

class AudioEngine {
    public:
        AudioEngine();
        ~AudioEngine();

        void start();
        void stop();

        void changeOutputDevice(int deviceId);

        std::vector<std::unique_ptr<AEffect>> &getEffects();

        char *getOutputDevicesAvailable();

        void setApiClient(ApiClient *apiClient) {
            _apiClient = apiClient;
        }

        ApiClient *getApiClient() {
            return _apiClient;
        }

        void applyVolumeInput(float *inputBuffer, unsigned int nBufferFrames);

        void applyVolumeOutput(float *outputBuffer, unsigned int nBufferFrames);

        void setVolumeInput(float volume) {
            volumeInput = volume;
        }

        void setVolumeOutput(float volume) {
            volumeOutput = volume;
        }

    private:
        #ifdef __linux__
            RtAudio adc = RtAudio(RtAudio::Api::LINUX_PULSE);
        #else
            RtAudio adc = RtAudio(RtAudio::Api::MACOSX_CORE);
        #endif
        RtAudio::StreamParameters inputParams;
        RtAudio::StreamParameters outputParams;
        bool already_draw = false;

        unsigned int bufferFrames = 512;
        unsigned int sampleRate = 44100;

        float volumeInput = 1.0f;
        float volumeOutput = 1.0f;

        std::vector<std::unique_ptr<AEffect>> effects;

        ApiClient *_apiClient;

        static int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
                         double streamTime, RtAudioStreamStatus status, void *userData);

        void findShowTimeVirtualDevice();

        void loadEffects();

        void processEffects(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames);

};
