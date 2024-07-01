cmake_minimum_required(VERSION 3.16..3.22)

project(
        showtime-audio-backend
        VERSION 0.1
        LANGUAGES CXX C)



add_executable(showtime-audio-backend)

target_compile_features(showtime-audio-backend PRIVATE cxx_std_17)
target_compile_options(showtime-audio-backend PRIVATE ${CMAJ_WARNING_FLAGS})

target_sources(showtime-audio-backend
        PRIVATE
        src/main.cpp
        src/AudioEngine.cpp
        src/AudioEngine.hpp
        src/Effects/IEffect.hpp
        src/Effects/AEffect.hpp
        src/Effects/GainEffect.hpp
        src/Effects/ReverbEffect.hpp)

find_package(RtAudio REQUIRED)
target_link_libraries(showtime-audio-backend
        PRIVATE
        RtAudio::rtaudio
        $<$<AND:$<CXX_COMPILER_ID:GNU>,$<VERSION_LESS:$<CXX_COMPILER_VERSION>,9.0>>:stdc++fs>
)
