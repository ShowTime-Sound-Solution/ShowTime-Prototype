//
// Created by Ewen TALLEUX on 30/06/2024.
//

#pragma once

#include "AEffect.hpp"
#include <cmath>
#include <vector>
#include <fftw3.h>

class EqualizerEffect : public AEffect {
    public:
        explicit EqualizerEffect(unsigned short const id) {
            setId(id);
        }

        void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
            if (!isEnabled()) {
                for (unsigned int i = 0; i < nBufferFrames; i++) {
                    outputBuffer[i] = inputBuffer[i];
                }
                return;
            }
        }

    private:
};
