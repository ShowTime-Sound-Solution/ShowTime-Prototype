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
        if (!isEnabled()) {
            for (unsigned int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            return;
        }
        if (count < 3) {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            count++;
        } else {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i] * 0.8 + impulseResponse3[i] * 0.4f;
            }
        }
        for (int i = 0; i < nBufferFrames; i++) {
            impulseResponse3[i] = impulseResponse2[i];
            impulseResponse2[i] = impulseResponse1[i];
            impulseResponse1[i] = outputBuffer[i];
        }
    }

private:
    float *impulseResponse1 = new float[512 * 2];
    float *impulseResponse2 = new float[512 * 2];
    float *impulseResponse3 = new float[512 * 2];
    int count = 0;
};