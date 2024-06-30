//
// Created by Ewen TALLEUX on 27/06/2024.
//

#pragma once
#include "AEffect.hpp"

class GainEffect : public AEffect {
    public:
        explicit GainEffect(unsigned short const id) {
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
                outputBuffer[i] = inputBuffer[i] * _gain;
            }
        }

        void setGain(float gain) {
            this->_gain = gain;
            std::cout << "Gain set to " << gain << std::endl;
        }

        [[nodiscard]] float getGain() const {
            return _gain;
        }

    private:
        float _gain = 0.3f;
};
