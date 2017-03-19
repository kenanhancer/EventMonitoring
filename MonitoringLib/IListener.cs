using System;
using System.Threading.Tasks;

namespace MonitoringLib
{
    public interface IListener : IDisposable
    {
        int ReceivedRequestCount { get; }
        Task AcceptClientsAsync(Action<string> callback = null);
    }
}