//
// Created by Ewen TALLEUX on 27/06/2024.
//

#pragma once

class IEffect {
    public:
        virtual ~IEffect() = default;
        virtual void process(float *inputBuffer, float *outputBuffer, unsigned int nBufferFrames) = 0;
    protected:
        unsigned short _id = 0;
        bool _enabled = false;
        std::string _name;
};
