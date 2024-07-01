//
// Created by Ewen TALLEUX on 01/07/2024.
//

#pragma once

#include "AEffect.hpp"
#include <iostream>

class PanEffect : public AEffect {
    public:
        explicit PanEffect(unsigned short const id) {
            setId(id);
        }

        void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) override {
            if (!isEnabled()) {
                for (unsigned int i = 0; i < nBufferFrames; i++) {
                    outputBuffer[i] = inputBuffer[i];
                }
                return;
            }
            for (unsigned int i = 0; i < nBufferFrames; i++) {
                if (_pan < 0) {
                    if (i % 2 == 0) {
                        outputBuffer[i] = inputBuffer[i] * (1 + _pan);
                    } else {
                        outputBuffer[i] = inputBuffer[i];
                    }
                } else {
                    if (i % 2 == 1) {
                        outputBuffer[i] = inputBuffer[i] * (1 - _pan);
                    } else {
                        outputBuffer[i] = inputBuffer[i];
                    }
                }
            }
        }

        void setPan(float pan) {
            this->_pan = pan;
            std::cout << "Pan set to " << pan << std::endl;
        }

        [[nodiscard]] float getPan() const {
            return _pan;
        }

    private:
        float _pan = 1.0f;
};
