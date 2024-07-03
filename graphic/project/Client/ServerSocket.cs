using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace project.Client;

public class ServerSocket
{
    private IPHostEntry _ipHost;
    private IPAddress _ipAddr;
    private IPEndPoint _localEndPoint;
    private Socket _listener;
    private Socket _client;
    private byte[] _buffer = new byte[4096];
    private Action<byte[]> _callback;
    
    public bool Connected { get; set; } = false;
    
    public ServerSocket()
    {
        _ipHost = Dns.GetHostEntry(Dns.GetHostName());
        _ipAddr = new IPAddress(
            new byte[]
            {
                127, 0, 0, 1
            });

        
        foreach (var ee in _ipHost.AddressList)
        {
            Console.WriteLine(ee);
        }

        _localEndPoint = new IPEndPoint(_ipAddr, 6942);
    }

    public void SetReceiveCallback(Action<byte[]> callback)
    {
        _callback = callback;
    }
    
    public void Start()
    {
        _listener = new Socket(_ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        try
        {
            _listener.Bind(_localEndPoint);
            _listener.Listen(10);
            Console.WriteLine("Waiting for a connection...");
            _client = _listener.Accept();
            Console.WriteLine("Connection established");
            Connected = true;
            LoopReceive();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void LoopReceive()
    {
        while (Connected)
        {
            Array.Fill(_buffer, byte.MinValue);

            try
            {
                _client.Receive(_buffer);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket closed");
                return;
            }
            if (Connected)
                _callback(_buffer);
        }
    }
    
    public void Send(byte[] data)
    {
        _client.SendAsync(data);
    }
    
    public void WaitForConnection()
    {
        while (!Connected)
        {
            Thread.Sleep(100);
        }
    }
    
    public void Stop()
    {
        _client.Shutdown(SocketShutdown.Both);
        _client.Close();
        _listener.Close();
    }
}