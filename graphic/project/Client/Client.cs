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

    public ObservableCollection<KeyValuePair<int, string>> Devices
    {
        get;
        private set;
    } = new ObservableCollection<KeyValuePair<int, string>>();
    
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
                string res = Encoding.ASCII.GetString(array);
                Devices.Clear();

                foreach (var line in res.Split('\n'))
                {
                    string[] parts = line.Split(" - ");
                    if (parts.Length == 6)
                        Devices.Add(new KeyValuePair<int, string>(int.Parse(parts[0]), parts[2]));
                }

            });
    }
}