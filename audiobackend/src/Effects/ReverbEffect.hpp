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
        delete[] impulseResponse9;
        delete[] impulseResponse10;
    }

    void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
        if (!isEnabled()) {
            for (unsigned int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            return;
        }
        for (int i = 0; i < nBufferFrames; i++) {
            if (inputBuffer[i] > abs(maxValueInBuffer)) {
                maxValueInBuffer = abs(inputBuffer[i]);
            }
        }
        if (count < 10) {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            count++;
        } else {
            for (int i = 0; i < nBufferFrames; i++) {
                if (abs(array[selector][i]) >= (maxValueInBuffer * 0.7)) {
                    outputBuffer[i] = inputBuffer[i] * 0.9 + array[selector][i] * 0.25f;
                } else {
                    outputBuffer[i] = inputBuffer[i] * 0.9 + array[selector][i] * 0.45f;
                }
            }
        }
        for (int i = 0; i < nBufferFrames; i++) {
            impulseResponse10[i] = impulseResponse9[i];
            impulseResponse9[i] = impulseResponse8[i];
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

    void changeSelector(short newSelector) {
        selector = newSelector;
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
    float *impulseResponse9 = new float[512 * 2];
    float *impulseResponse10 = new float[512 * 2];
    float *array[10] = {impulseResponse1, impulseResponse2, impulseResponse3, impulseResponse4, impulseResponse5, impulseResponse6, impulseResponse7, impulseResponse8, impulseResponse9, impulseResponse10};
    short selector = 0;
    int count = 0;
    float maxValueInBuffer = 0;
};