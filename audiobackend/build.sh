git clone https://github.com/thestk/rtaudio.git
cd rtaudio
mkdir build
cd build
cmake ..
make
sudo cmake --install .
cd ../..
mkdir build
cd build
cmake ..
make
mv showtime-audio-backend ..
cd ..
sudo rm -rf rtaudio
sudo rm -rf build