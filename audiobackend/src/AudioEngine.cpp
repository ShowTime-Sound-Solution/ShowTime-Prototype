//
// Created by Ewen TALLEUX on 27/06/2024.
//

#include "AudioEngine.hpp"

AudioEngine::AudioEngine() {
    std::cout << "Initializing Audio Engine" << std::endl;
    findShowTimeVirtualDevice();

    outputParams.deviceId = adc.getDefaultOutputDevice();
    outputParams.nChannels = 2;
    outputParams.firstChannel = 0;

    std::cout << "Default output device: " << adc.getDeviceInfo(outputParams.deviceId).name << std::endl;
}

AudioEngine::~AudioEngine() {
    stop();
    if (adc.isStreamOpen()) {
        adc.closeStream();
    }
}

void AudioEngine::findShowTimeVirtualDevice() {
    auto devices = adc.getDeviceIds();
    if (devices.size() == 0) {
        std::cout << "No audio devices found" << std::endl;
        exit(1);
    }
    std::cout << "Found " << devices.size() << " audio devices" << std::endl;
    for (auto device : devices) {
        std::cout << "Device: " << adc.getDeviceInfo(device).name << std::endl;
        if (adc.getDeviceInfo(device).name == "ShowTime Sound Solution: ShowTime_Virtual_Input 2ch") {
            std::cout << "Found ShowTime Virtual Device" << std::endl;
            inputParams.deviceId = device;
            inputParams.nChannels = 2;
            inputParams.firstChannel = 0;

            return;
        }
    }
    std::cout << "ShowTime Virtual Device not found" << std::endl;
    exit(1);
}

void AudioEngine::start() {
    try {
        RtAudio::StreamOptions options;
        options.flags = RTAUDIO_NONINTERLEAVED;
        options.streamName = "ShowTime";
        adc.openStream(&outputParams, &inputParams, RTAUDIO_FLOAT32, sampleRate, &bufferFrames, &input, this, &options);
        adc.startStream();
    } catch (...) {
        std::cout << "Error starting audio stream" << std::endl;
        exit(1);
    }
}

void AudioEngine::stop() {
    adc.stopStream();
}

void AudioEngine::setGain(float gain) {
    this->gain = gain;
}

int AudioEngine::input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames, double streamTime, RtAudioStreamStatus status, void *userData) {
    AudioEngine *engine = (AudioEngine *)userData;
    float *in = (float *)inputBuffer;
    float *out = (float *)outputBuffer;
    for (int i = 0; i < nBufferFrames * 2; i++) {
        out[i] = in[i] * engine->gain;
    }
    return 0;
}

void AudioEngine::changeOutputDevice(int numDevice) {
    stop();
    auto deviceId = 0;
    auto devices = adc.getDeviceIds();
    for (int i = 0; i < devices.size(); i++) {
        if (i + 1 == numDevice) {
            deviceId = devices[i];
            break;
        }
    }
    adc.closeStream();
    auto device = adc.getDeviceInfo(deviceId);
    outputParams.deviceId = device.ID;
    outputParams.nChannels = device.outputChannels;
    outputParams.firstChannel = 0;
    adc.openStream(&outputParams, &inputParams, RTAUDIO_FLOAT32, sampleRate, &bufferFrames, &input, this);
    adc.startStream();
}


