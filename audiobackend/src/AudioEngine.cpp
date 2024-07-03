//
// Created by Ewen TALLEUX on 27/06/2024.
//

#include "AudioEngine.hpp"
#include "API/ApiClient.hpp"

AudioEngine::AudioEngine() {
    //check if linux
    #ifdef __linux__
        std::cout << "Linux detected" << std::endl;
        system("pactl load-module module-null-sink sink_name=ShowTime_Virtual_Input_1 sink_properties=device.description=ShowTime_Virtual_Input_1 channels=2");
        //change api to pulse
        adc.showWarnings(true);
    #endif

    std::cout << "Initializing Audio Engine" << std::endl;
    std::cout << "Audio API used: " << adc.getCurrentApi() << std::endl;
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
    #ifndef __linux__
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
    #else
        for (auto device : devices) {
            std::cout << "Device: " << adc.getDeviceInfo(device).name << " (" << device << ")" << std::endl;
            if (adc.getDeviceInfo(device).name == "Monitor of ShowTime_Virtual_Input_1") {
                std::cout << "Found ShowTime Virtual Device with " << adc.getDeviceInfo(device).inputChannels << " channels (" << device << ")" << std::endl;
                inputParams.deviceId = device;
                inputParams.nChannels = adc.getDeviceInfo(device).inputChannels;
                inputParams.firstChannel = 0;

                return;
            }
        }
    #endif
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
    engine->getApiClient()->sendInputBuffer((char *)in);
    engine->processEffects(in, out, nBufferFrames);
    engine->getApiClient()->sendOutputBuffer((char *)out);
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
    std::cout << "Output device changed to: " << adc.getDeviceInfo(deviceId).name << " with sample rate: " << sampleRate << " and " << device.outputChannels << " channels" << std::endl;
}

void AudioEngine::loadEffects() {
    effects.push_back(std::make_unique<GainEffect>(0));
    effects.push_back(std::make_unique<PhaseInverterEffect>(1));
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

std::vector<std::unique_ptr<AEffect>> &AudioEngine::getEffects() {
    return effects;
}

char *AudioEngine::getOutputDevicesAvailable() {
    auto devices = adc.getDeviceIds();
    auto buffer = new char[1024];
    memset(buffer, 0, 1024);
    for (int i = 0; i < devices.size(); i++) {
        auto device = adc.getDeviceInfo(devices[i]);
        strcat(buffer, std::to_string(i + 1).c_str());
        strcat(buffer, " - ");
        strcat(buffer, std::to_string(device.ID).c_str());
        strcat(buffer, " - ");
        strcat(buffer, device.name.c_str());
        strcat(buffer, " - ");
        strcat(buffer, std::to_string(device.outputChannels).c_str());
        strcat(buffer, " - ");
        strcat(buffer, std::to_string(device.preferredSampleRate).c_str());
        strcat(buffer, " - ");
        strcat(buffer, std::to_string(device.inputChannels).c_str());
        strcat(buffer, "\n");
    }
    return buffer;
}

char *AudioEngine::getEffectsAvailable()
{
    auto buffer = new char[1024];
    memset(buffer, 0, 1024);
    for (auto &effect : effects) {
        strcat(buffer, std::to_string(effect->getId()).c_str());
        strcat(buffer, " - ");
        strcat(buffer, effect->getName().c_str());
        strcat(buffer, " - ");
        strcat(buffer, effect->isEnabled() ? "enabled" : "disabled");
        strcat(buffer, "\n");
    }
    return buffer;
}
