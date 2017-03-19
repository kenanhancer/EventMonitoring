using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringLib
{
    public class BaseListener : IListener, IDisposable
    {
        public string Ip { get; private set; }
        public string PortNumber { get; private set; }
        public int ReceiveBufferSize { get; private set; }
        public int BacklogClientCount { get; private set; }
        public CancellationToken Token { get; private set; }
        public virtual int ReceivedRequestCount => 0;
        protected TcpListener tcpListener;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">127.0.0.1</param>
        /// <param name="portNumber">8080</param>
        /// <param name="receiveBufferSize">1024 * 8 (8kb)</param>
        /// <param name="backlogClientCount">100</param>
        /// <param name="token"></param>
        public BaseListener(string ip, string portNumber, int receiveBufferSize, int backlogClientCount, CancellationToken token)
        {
            Ip = ip;
            PortNumber = portNumber;
            ReceiveBufferSize = receiveBufferSize == 0 ? (8 * 1024) : receiveBufferSize;
            BacklogClientCount = backlogClientCount;
            Token = token;

            IPAddress ipAddress = Helper.GetIpAdress(Ip);

            int port = Helper.GetPort(PortNumber);

            tcpListener = new TcpListener(ipAddress, port);
        }

        public virtual Task AcceptClientsAsync(Action<string> callback)
        {
            return Task.CompletedTask;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TcpListener tcpListener_ = tcpListener;
                    tcpListener = null;
                    if (tcpListener_ != null)
                    {
                        tcpListener_.Stop();
                        tcpListener_ = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}