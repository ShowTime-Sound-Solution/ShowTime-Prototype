#include "AudioEngine.hpp"
#include "API/ApiClient.hpp"
#include <iostream>
#include <cstdlib>
#include <sys/stat.h>

// Callback function to process audio data
/*int input(void *outputBuffer, void *inputBuffer, unsigned int nBufferFrames,
          double streamTime, RtAudioStreamStatus status, void *userData) {
    if (status)
        std::cout << "Stream overflow detected!" << std::endl;

    auto *in = static_cast<float *>(inputBuffer);
    auto *out = static_cast<float *>(outputBuffer);

    auto max_left = 0.0f;
    auto max_right = 0.0f;

    if (already_draw)
        std::cout << "\033[2J\033[1;1H";

    for (unsigned int i = 0; i < nBufferFrames * 2; i++) {
        out[i] = in[i] * gain;
        if (i % 2 == 0) {
            if (out[i] > max_left) {
                max_left = out[i];
            }
        } else {
            if (out[i] > max_right) {
                max_right = out[i];
            }
        }
    }

    for (int i = 0; i < 10; i++) {
        if (max_left > i * 0.1) {
            std::cout << "*";
        } else {
            std::cout << " ";
        }
    }
    std::cout << std::endl;
    for (int i = 0; i < 10; i++) {
        if (max_right > i * 0.1) {
            std::cout << "*";
        } else {
            std::cout << " ";
        }
    }
    std::cout << std::endl;

    if (!already_draw) {
        already_draw = true;
    }

    return 0;
}*/

int main() {
    std::shared_ptr<AudioEngine> ae = std::make_shared<AudioEngine>();
    ApiClient apiClient(ae);

    apiClient.run();

    ae->start();

    char input;

    std::cout << "\nPlaying ... press 'q' and <enter> to quit.\n";

    //enter while quite and p while add gain
    GainEffect *gainEffect = nullptr;
    for (auto &effect : ae->getEffects()) {
        gainEffect = dynamic_cast<GainEffect *>(effect.get());
    }
    /*while (std::cin.get(input)) {
        if (input == 'p' && gainEffect != nullptr) {
            gainEffect->setGain(gainEffect->getGain() + 0.1f);
            std::cout << "Gain: " << gainEffect->getGain() << std::endl;
        }
        if (input == 'm' && gainEffect != nullptr) {
            gainEffect->setGain(gainEffect->getGain() - 0.1f);
            std::cout << "Gain: " << gainEffect->getGain() << std::endl;
        }
        if (input == 'q') {
            break;
        }
        if (input == '1') {
            ae.changeOutputDevice(1);
        }
        if (input == '2') {
            ae.changeOutputDevice(2);
        }
        if (input == '3') {
            ae.changeOutputDevice(3);
        }
        if (input == '4') {
            ae.changeOutputDevice(4);
        }
        if (input == '5') {
            ae.changeOutputDevice(5);
        }
    }*/

    return 0;
}