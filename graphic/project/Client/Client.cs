using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Threading;

namespace project.Client;

public class Client
{
    ServerSocket _serverSocket;
    Thread _thread;
    private Dictionary<int, Action<byte[]>> _parseFunctions;

    public Client()
    {
        _serverSocket = new ServerSocket();
        _parseFunctions = new Dictionary<int, Action<byte[]>>() {{0x01, ParseAvailableDevice}, {0x03, ParseAudioBuffer}};
        
        _thread = new Thread(() =>
        {
            _serverSocket.Start();
        });
        _serverSocket.SetReceiveCallback(ParseBuffer);
        _thread.Start();
        _serverSocket.WaitForConnection();
        AskAvailableDevices();
    }

    public void AskAvailableDevices()
    {
        _serverSocket.Send(new byte[] {0x01});
    }
    
    private void ParseBuffer(byte[] buffer)
    {
        int id = buffer[0];
        
        if (_parseFunctions.ContainsKey(id))
        {
            _parseFunctions[id](buffer.Skip(1).ToArray());
        }
    }

    private void ParseAvailableDevice(byte[] array)
    {

        Dispatcher.UIThread.Invoke(
            () =>
            {
                AvailableDeviceEventHandler handler = AvailableDeviceEvent;
                
                Dictionary<int, string> args = new Dictionary<int, string>();
                string res = Encoding.ASCII.GetString(array);

                foreach (var line in res.Split('\n'))
                {
                    string[] parts = line.Split(" - ");
                    if (parts.Length == 6)
                        args.Add(int.Parse(parts[0]), parts[2]);
                }

                handler(this, args);
            });
    }

    public void SelectDevice(int id)
    {
        _serverSocket.Send(new byte[] {0x02, (byte) id});
    }

    public void Stop()
    {
        if (_serverSocket.Connected)
        {
            _serverSocket.Send(Encoding.ASCII.GetBytes("stop\n"));
        }
        _serverSocket.Stop();
        _thread.Interrupt();
    }

    private void ParseAudioBuffer(byte[] array)
    {

        Dispatcher.UIThread.Invoke(
            () =>
            {
                var handler = AudioBufferEvent;

                handler(this, array);
            });
    }
    
    public void SendInputFaderValue(float value)
    {
        _serverSocket.Send(new byte[] {0x09}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public void SendOutputFaderValue(float value)
    {
        _serverSocket.Send(new byte[] {0x0A}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public void SendGainValue(float value)
    {
        _serverSocket.Send(new byte[] {0x06}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public void SendPanValue(float value)
    {
        _serverSocket.Send(new byte[] {0x08}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public void SendReverbValue(short value)
    {
        _serverSocket.Send(new byte[] {0x07}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public void SendEnableEffect(int value)
    {
        _serverSocket.Send(new byte[] {0x05}.Concat(BitConverter.GetBytes(value)).ToArray());
    }
    
    public event AvailableDeviceEventHandler AvailableDeviceEvent;
    public event AudioBufferEventHandler AudioBufferEvent;
}

public delegate void AvailableDeviceEventHandler(object sender, Dictionary<int, string> args);
public delegate void AudioBufferEventHandler(object sender, byte[] buffer);

public class AvailableDeviceEventHandlerArgs : EventArgs
{
    public Dictionary<int, string> Devices { get; set; }
}