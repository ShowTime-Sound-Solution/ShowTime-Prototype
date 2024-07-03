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
        _parseFunctions = new Dictionary<int, Action<byte[]>>() {{0x01, ParseAvailableDevice}};
        
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
    
    public event AvailableDeviceEventHandler AvailableDeviceEvent;
}

public delegate void AvailableDeviceEventHandler(object sender, Dictionary<int, string> args);

public class AvailableDeviceEventHandlerArgs : EventArgs
{
    public Dictionary<int, string> Devices { get; set; }
}