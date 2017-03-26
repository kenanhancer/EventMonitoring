using System;
using System.IO;
using System.Linq;
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

            int bytesRead = 0;

            long totalBytesRead = 0;

            byte[] bytesBuffer = new byte[ReceiveBufferSize];

            MemoryStream memoryStream = new MemoryStream();

            string contentLengthStr = "Content-Length: ";

            string contentLengthValue;

            int contentLength;

            byte[] responseBuffer;

            int indexOfContentLength;

            while (true)
            {
                try
                {
                    using (acceptedTcpClient = await tcpListener.AcceptTcpClientAsync().ContinueWith(t => t.Result, Token))
                    {
                        using (NetworkStream networkStream = acceptedTcpClient.GetStream())
                        {
                            Interlocked.Increment(ref receivedRequestCount);

                            totalBytesRead = 0;

                            string requestReceived = String.Empty;

                            //memoryStream = new MemoryStream();

                            bytesRead = await networkStream.ReadAsync(bytesBuffer, 0, ReceiveBufferSize, Token);

                            requestReceived = Encoding.UTF8.GetString(bytesBuffer, 0, bytesRead);

                            //await memoryStream.WriteAsync(bytesBuffer, 0, bytesRead, Token);

                            indexOfContentLength = requestReceived.IndexOf(contentLengthStr) + 16;// "Content-Length: ".Length;

                            contentLengthValue = requestReceived.Substring(indexOfContentLength, requestReceived.IndexOf("\r\n", indexOfContentLength) - indexOfContentLength);

                            contentLength = int.Parse(contentLengthValue);

                            int headerLastIndex = requestReceived.IndexOf("\r\n\r\n") + 4;

                            string body = requestReceived.Substring(headerLastIndex);

                            if (contentLength > 0 && String.IsNullOrEmpty(body))
                            {
                                while (totalBytesRead < contentLength)
                                {
                                    bytesRead = await networkStream.ReadAsync(bytesBuffer, 0, ReceiveBufferSize, Token);

                                    totalBytesRead += bytesRead;

                                    requestReceived += Encoding.UTF8.GetString(bytesBuffer, 0, bytesRead);

                                    //await memoryStream.WriteAsync(bytesBuffer, 0, bytesRead, Token);
                                }

                                body = requestReceived.Substring(headerLastIndex);
                            }

                            responseBuffer = Helper.GetBodyBuffer("HTTP/1.1", "application/json", 0, "200 OK", "");

                            await networkStream.WriteAsync(responseBuffer, 0, responseBuffer.Length, Token);

                            if (callback != null)
                                Task.Run(() => callback(body));
                        }
                    }
                }
                catch (SocketException ex)
                {
                    await Console.Out.WriteLineAsync("SERVER ------" + ex.ToString());
                }
            }
        }
    }
}