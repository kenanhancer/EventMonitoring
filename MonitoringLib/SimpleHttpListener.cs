using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringLib
{
    public class SimpleHttpListener : BaseListener
    {
        int receivedRequestCount = 0;
        public override int ReceivedRequestCount => receivedRequestCount;

        public SimpleHttpListener(string ip, string portNumber, int receiveBufferSize, int backlogClientCount, CancellationToken token) : base(ip, portNumber, receiveBufferSize, backlogClientCount, token)
        {
        }

        public override async Task AcceptClientsAsync(Action<string> callback = null)
        {
            tcpListener.Start(BacklogClientCount);//backlog 1000 clients.

            TcpClient acceptedTcpClient;

            int bytesRead;

            long totalBytesRead = 0;

            byte[] bytesBuffer = new byte[ReceiveBufferSize];

            while (true)
            {
                try
                {
                    acceptedTcpClient = await tcpListener.AcceptTcpClientAsync().ContinueWith(t => t.Result, Token);

                    using (NetworkStream networkStream = acceptedTcpClient.GetStream())
                    {
                        string requestReceived = String.Empty;

                        bytesRead = await networkStream.ReadAsync(bytesBuffer, 0, ReceiveBufferSize, Token);

                        requestReceived += Encoding.UTF8.GetString(bytesBuffer, 0, bytesRead);

                        if (String.IsNullOrEmpty(requestReceived))
                            continue;

                        string[] splitRequest = requestReceived.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        //Performance boost
                        //if (splitRequest.Length == 0  || !splitRequest[0].Contains("HTTP"))
                        //    continue;

                        if (splitRequest.Length == 5)
                        {
                            bytesRead = await networkStream.ReadAsync(bytesBuffer, 0, ReceiveBufferSize, Token);

                            requestReceived += Encoding.UTF8.GetString(bytesBuffer, 0, bytesRead);

                            splitRequest = requestReceived.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        }

                        string body = splitRequest[splitRequest.Length - 1];

                        byte[] responseBuffer = Helper.GetBodyBuffer("HTTP/1.1", "application/json", 0, "200 OK", "");

                        await networkStream.WriteAsync(responseBuffer, 0, responseBuffer.Length, Token);

                        Interlocked.Increment(ref receivedRequestCount);

                        callback?.Invoke(body);
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