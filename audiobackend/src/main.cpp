#include "RtAudio.h"
#include <CoreAudio/CoreAudio.h>
#include <iostream>
#include <cstdlib>
#include <sys/stat.h>

bool already_draw = false;

float gain = 1.0f;

// Callback function to process audio data
int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
          double streamTime, RtAudioStreamStatus status, void *userData) {
    if (status)
        std::cout << "Stream overflow detected!" << std::endl;

    auto *in = static_cast<float *>(inputBuffer);
    auto *out = static_cast<float *>(outputBuffer);

    auto max_left = 0.0f;
    auto max_right = 0.0f;

    if (already_draw)
        std::cout << "\033[2J\033[1;1H";

    for (unsigned int i = 0; i < nBufferFrames * 2; i++) {
        out[i] = in[i] * gain;
        if (i % 2 == 0) {
            if (out[i] > max_left) {
                max_left = out[i];
            }
        } else {
            if (out[i] > max_right) {
                max_right = out[i];
            }
        }
    }

    for (int i = 0; i < 10; i++) {
        if (max_left > i * 0.1) {
            std::cout << "*";
        } else {
            std::cout << " ";
        }
    }
    std::cout << std::endl;
    for (int i = 0; i < 10; i++) {
        if (max_right > i * 0.1) {
            std::cout << "*";
        } else {
            std::cout << " ";
        }
    }
    std::cout << std::endl;

    if (!already_draw) {
        already_draw = true;
    }

    return 0;
}

int main() {
    RtAudio adc;
    if (adc.getDeviceCount() < 1) {
        std::cout << "\nNo audio devices found!\n";
        exit(0);
    }
    RtAudio::StreamParameters parameters;
    parameters.deviceId = 133;
    parameters.nChannels = 2;
    parameters.firstChannel = 0;
    unsigned int sampleRate = 44100;
    unsigned int bufferFrames = 256; // 256 sample frames

    RtAudio::StreamParameters out;
    out.deviceId = 129;
    out.nChannels = 2;
    out.firstChannel = 0;

    std::cout << "Default input device: " << adc.getDeviceInfo(parameters.deviceId).name << " " << parameters.deviceId << std::endl;

    //list all available devices
    auto devices = adc.getDeviceIds();

    for (auto device : devices) {
        std::cout << "Device: " << adc.getDeviceInfo(device).name << " " << device << " " << adc.getDeviceInfo(device).inputChannels << " " << adc.getDeviceInfo(device).outputChannels << std::endl;
    }

    try {
        adc.openStream(&out, &parameters, RTAUDIO_FLOAT32,
                       sampleRate, &bufferFrames, &input, (void *)NULL);
        adc.startStream();
    }
    catch (...) {
        std::cout << "Error starting stream\n";
        exit(0);
    }


    char input;

    std::cout << "\nPlaying ... press <enter> to quit.\n";

    //enter while quite and p while add gain
    while (std::cin.get(input)) {
        if (input == 'p') {
            gain += 0.1f;
            std::cout << "Gain: " << gain << std::endl;
        }
        if (input == 'm') {
            gain -= 0.1f;
            std::cout << "Gain: " << gain << std::endl;
        }
        if (input == 'q') {
            break;
        }
    }



    try {
        // Stop the stream
        adc.stopStream();
    }
    catch (...) {
        std::cout << "Error stopping stream\n";
    }
    if (adc.isStreamOpen()) adc.closeStream();
    return 0;
}