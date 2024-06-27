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

    sampleRate = adc.getDeviceInfo(outputParams.deviceId).preferredSampleRate;

    std::cout << "Default output device: " << adc.getDeviceInfo(outputParams.deviceId).name << std::endl;
    loadEffects();
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

int AudioEngine::input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames, double streamTime, RtAudioStreamStatus status, void *userData) {
    AudioEngine *engine = (AudioEngine *)userData;
    float *in = (float *)inputBuffer;
    float *out = (float *)outputBuffer;
    engine->processEffects(in, out, nBufferFrames);
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
    std::cout << "Changing output device to: " << adc.getDeviceInfo(deviceId).name << std::endl;
    adc.closeStream();
    auto device = adc.getDeviceInfo(deviceId);
    outputParams.deviceId = device.ID;
    outputParams.nChannels = device.outputChannels;
    outputParams.firstChannel = 0;
    sampleRate = device.preferredSampleRate;
    adc.openStream(&outputParams, &inputParams, RTAUDIO_FLOAT32, sampleRate, &bufferFrames, &input, this);
    adc.startStream();
    std::cout << "Output device changed to: " << adc.getDeviceInfo(deviceId).name << " with sample rate: " << sampleRate << std::endl;
}

void AudioEngine::loadEffects() {
    effects.push_back(std::make_unique<GainEffect>(0));
}

void AudioEngine::processEffects(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) {
    float *tmpBuffer = new float[nBufferFrames * 2];
    for (auto &effect : effects) {
        effect->process(inputBuffer, tmpBuffer, nBufferFrames * 2);
        std::copy(tmpBuffer, tmpBuffer + nBufferFrames * 2, inputBuffer);
    }
    delete[] tmpBuffer;
    for (unsigned int i = 0; i < nBufferFrames * 2; i++) {
        outputBuffer[i] = inputBuffer[i];
    }
}

std::vector<std::unique_ptr<IEffect>> &AudioEngine::getEffects() {
    return effects;
}
