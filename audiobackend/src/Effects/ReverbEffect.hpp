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
        _enabled = true;
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
        delete[] impulseResponse11;
        delete[] impulseResponse12;
        delete[] impulseResponse13;
        delete[] impulseResponse14;
        delete[] impulseResponse15;
        delete[] impulseResponse16;
        delete[] impulseResponse17;
        delete[] impulseResponse18;
        delete[] impulseResponse19;
        delete[] impulseResponse20;
    }

    void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
        for (int i = 0; i < nBufferFrames; i++) {
            impulseResponse20[i] = impulseResponse19[i];
            impulseResponse19[i] = impulseResponse18[i];
            impulseResponse18[i] = impulseResponse17[i];
            impulseResponse17[i] = impulseResponse16[i];
            impulseResponse16[i] = impulseResponse15[i];
            impulseResponse15[i] = impulseResponse14[i];
            impulseResponse14[i] = impulseResponse13[i];
            impulseResponse13[i] = impulseResponse12[i];
            impulseResponse12[i] = impulseResponse11[i];
            impulseResponse11[i] = impulseResponse10[i];
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
        if (count < 20) {
            for (int i = 0; i < nBufferFrames; i++) {
                outputBuffer[i] = inputBuffer[i];
            }
            count++;
        } else {
            for (int i = 0; i < nBufferFrames; i++) {
                if (abs(array[selector][i]) >= (maxValueInBuffer * 0.7)) {
                    outputBuffer[i] = inputBuffer[i] * 0.9 + array[selector][i] * 0.15f;
                } else {
                    outputBuffer[i] = inputBuffer[i] * 0.9 + array[selector][i] * 0.35f;
                }
            }
        }
    }

    void changeSelector(short newSelector) {
        if (newSelector < 0 || newSelector > 19) {
            return;
        }
        std::cout << "Rev selector changed to " << newSelector << std::endl;
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
    float *impulseResponse11 = new float[512 * 2];
    float *impulseResponse12 = new float[512 * 2];
    float *impulseResponse13 = new float[512 * 2];
    float *impulseResponse14 = new float[512 * 2];
    float *impulseResponse15 = new float[512 * 2];
    float *impulseResponse16 = new float[512 * 2];
    float *impulseResponse17 = new float[512 * 2];
    float *impulseResponse18 = new float[512 * 2];
    float *impulseResponse19 = new float[512 * 2];
    float *impulseResponse20 = new float[512 * 2];
    float *array[20] = {impulseResponse1, impulseResponse2, impulseResponse3, impulseResponse4, impulseResponse5, impulseResponse6, impulseResponse7, impulseResponse8, impulseResponse9, impulseResponse10, impulseResponse11, impulseResponse12, impulseResponse13, impulseResponse14, impulseResponse15, impulseResponse16, impulseResponse17, impulseResponse18, impulseResponse19, impulseResponse20};
    short selector = 0;
    int count = 0;
    float maxValueInBuffer = 0;
};