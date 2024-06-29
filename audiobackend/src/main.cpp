#include "AudioEngine.hpp"
#include "API/ApiClient.hpp"
#include <iostream>

int main() {
    std::shared_ptr<AudioEngine> ae = std::make_shared<AudioEngine>();
    ApiClient apiClient(ae);

    apiClient.run();

    ae->start();

    std::cout << "\nPlaying ... press 'q' and <enter> to quit.\n";

    return 0;
}