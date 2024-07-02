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

    ~ReverbEffect() {
        delete[] impulseResponse1;
        delete[] impulseResponse2;
        delete[] impulseResponse3;
        delete[] impulseResponse4;
        delete[] impulseResponse5;
        delete[] impulseResponse6;
        delete[] impulseResponse7;
        delete[] impulseResponse8;
    }

    void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
        if (!isEnabled()) {
            for (unsigned int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            return;
        }
        if (count < 8) {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            count++;
        } else {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i] * 0.9 + impulseResponse8[i] * 0.5f;
            }
        }
        for (int i = 0; i < nBufferFrames; i++) {
            impulseResponse8[i] = impulseResponse7[i];
            impulseResponse7[i] = impulseResponse6[i];
            impulseResponse6[i] = impulseResponse5[i];
            impulseResponse5[i] = impulseResponse4[i];
            impulseResponse4[i] = impulseResponse3[i];
            impulseResponse3[i] = impulseResponse2[i];
            impulseResponse2[i] = impulseResponse1[i];
            impulseResponse1[i] = outputBuffer[i];
        }
    }

private:
    float *impulseResponse1 = new float[512 * 2];
    float *impulseResponse2 = new float[512 * 2];
    float *impulseResponse3 = new float[512 * 2];
    float *impulseResponse4 = new float[512 * 2];
    float *impulseResponse5 = new float[512 * 2];
    float *impulseResponse6 = new float[512 * 2];
    float *impulseResponse7 = new float[512 * 2];
    float *impulseResponse8 = new float[512 * 2];
    int count = 0;
};