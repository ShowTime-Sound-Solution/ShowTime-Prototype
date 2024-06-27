//
// Created by Ewen TALLEUX on 28/06/2024.
//

#pragma once
#include "IEffect.hpp"

class AEffect : public IEffect {
    public:
        void setId(unsigned short id) {
            _id = id;
        }

        [[nodiscard]] unsigned short getId() const {
            return _id;
        }
};