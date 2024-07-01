/*
** EPITECH PROJECT, 2024
** showtime-audio-backend
** File description:
** ReverbEffect.hpp
*/

#pragma once
#include "AEffect.hpp"
#include <vector>

class ReverbEffect : public AEffect {
public:
    explicit ReverbEffect(unsigned short const id) {
        setId(id);
    }

    void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
        if (impulseResponse == nullptr) {
            impulseResponse = new float[nBufferFrames];
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
        } else {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i] + impulseResponse[i] * 0.5f;
            }
        }
        for (int i = 0; i < nBufferFrames; i++) {
            impulseResponse[i] = outputBuffer[i];
        }
    }


private:
    float *impulseResponse = nullptr;
};