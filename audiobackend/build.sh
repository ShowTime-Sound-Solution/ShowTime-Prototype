#install dep, if it is ubuntu it is apt, if it is fedora it is dnf
OS_NAME=$(lsb_release -si)

# Check if the distribution is Debian-based (uses apt)
if [ "$OS_NAME" == "Debian" ] || [ "$OS_NAME" == "Ubuntu" ]; then
  sudo apt-get install libpulse-dev autoconf automake libtool
elif [ "$OS_NAME" == "CentOS" ] || [ "$OS_NAME" == "Red Hat Enterprise Linux" ]; then
  sudo dnf install pulseaudio-libs-devel autoconf automake libtool
else
  echo "Unsupported distribution: $OS_NAME"
  exit 1
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
