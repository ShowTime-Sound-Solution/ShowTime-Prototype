//
// Created by Ewen TALLEUX on 27/06/2024.
//

#pragma once

#include "RtAudio.h"
#include <CoreAudio/CoreAudio.h>

class AudioEngine {
    public:
        AudioEngine();
        ~AudioEngine();

        void start();
        void stop();

        void setGain(float gain);
        float getGain() const { return gain; }

        void changeOutputDevice(int deviceId);

    private:
        RtAudio adc;
        RtAudio::StreamParameters inputParams;
        RtAudio::StreamParameters outputParams;
        bool already_draw = false;
        float gain = 1.0f;

        unsigned int bufferFrames = 512;
        unsigned int sampleRate = 44100;

        static int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
                         double streamTime, RtAudioStreamStatus status, void *userData);

        void findShowTimeVirtualDevice();

};
