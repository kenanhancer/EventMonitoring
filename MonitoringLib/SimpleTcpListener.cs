using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringLib
{
    public class SimpleTcpListener : BaseListener
    {
        public SimpleTcpListener(string ip, string portNumber, int receiveBufferSize, int backlogClientCount, CancellationToken token) : base(ip, portNumber, receiveBufferSize, backlogClientCount, token)
        {
        }

        public override async Task AcceptClientsAsync(Action<string> callback)
        {
            tcpListener.Start(1000);//backlog 1000 clients.

            TcpClient acceptedTcpClient;

            int bytesRead;

            long totalBytesRead = 0;

            byte[] bytesBuffer = new byte[ReceiveBufferSize];

            long clientCount = 0;

            while (true)
            {
                try
                {
                    acceptedTcpClient = await tcpListener.AcceptTcpClientAsync().ContinueWith(t => t.Result, Token);

                    Interlocked.Increment(ref clientCount);

                    using (NetworkStream networkStream = acceptedTcpClient.GetStream())
                    {
                        string requestReceived = String.Empty;

                        bytesRead = await networkStream.ReadAsync(bytesBuffer, 0, ReceiveBufferSize, Token);

                        requestReceived += Encoding.UTF8.GetString(bytesBuffer, 0, bytesRead);

                        if (String.IsNullOrEmpty(requestReceived))
                            continue;

                        callback?.Invoke(requestReceived);
                    }
                }
                catch (SocketException ex)
                {
                    await Console.Out.WriteLineAsync(ex.ToString());
                }
            }
        }
    }
}