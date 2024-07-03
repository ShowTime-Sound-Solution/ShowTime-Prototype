#install dep, if it is ubuntu it is apt, if it is fedora it is dnf
if [ "$(uname)" != "Darwin" ]; then
  OS_NAME=$(lsb_release -si)

  # Check if the distribution is Debian-based (uses apt)
  # if test "$OS_NAME" = "Ubuntu" -o "$OS_NAME" = "Debian"; then
  #   sudo apt-get install libpulse-dev autoconf automake libtool
  # elif test "$OS_NAME" = "Fedora"; then
  #   sudo dnf install pulseaudio-libs-devel autoconf automake libtool
  # elif
  #   test "$OS_NAME" = "Darwin"; then
  #   echo "MacOS"
  # else
  #   echo "Unsupported distribution: $OS_NAME"
  #   exit 1
  # fi 
fi
git clone https://github.com/thestk/rtaudio.git
cd rtaudio
if [ "$(uname)" != "Darwin" ]; then
  ./autogen.sh
  ./configure --with-pulse
fi
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
echo "Installing virtual audio device drivers..."
## checking if the drivers are already installed if on macos
if [ "$(uname)" != "Darwin" ]; then
    echo "Not on MacOS"
else
    echo "On MacOS, checking if drivers are installed..."
    if [ -d "/Library/Audio/Plug-Ins/HAL/BlackHole.driver" ]; then
        echo "Drivers already installed"
    else
        echo "Installing drivers..."
        cd drivers
        mkdir ShowTime.driver
        cd ..
        cp -r drivers/BlackHole.driver/ drivers/ShowTime.driver/
        mv drivers/BlackHole.driver/ /Library/Audio/Plug-Ins/HAL/
        mv drivers/ShowTime.driver/ drivers/BlackHole.driver/
    fi
    sudo killall -9 coreaudiod
fi
echo "Done"
