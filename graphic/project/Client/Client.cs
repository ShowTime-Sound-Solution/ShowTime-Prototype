using System.Text;
using System.Threading;

namespace project.Client;

public class Client
{
    ServerSocket _serverSocket;
    Thread _thread;
    
    public Client()
    {
        _serverSocket = new ServerSocket();
        
        _thread = new Thread(() =>
        {
            _serverSocket.Start();
        });
        _thread.Start();
        _serverSocket.WaitForConnection();
        AskAvailableDevices();
    }

    public void AskAvailableDevices()
    {
        _serverSocket.Send(new byte[] {0x01});
    }
}