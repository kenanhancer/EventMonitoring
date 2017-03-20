using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonitoringLib
{
    public class SimpleMonitoringServer : IDisposable
    {
        IListener simpleListener;
        ICache cache;
        string eventKeyFieldName;

        public int ReceivedRequestCount => simpleListener.ReceivedRequestCount;

        public SimpleMonitoringServer(string ip, string portNumber, ICache cache, string eventKeyFieldName, CancellationToken token)
        {
            simpleListener = new SimpleHttpListener(ip, portNumber, 8 * 1024, 1000, token);
            this.cache = cache;
            this.eventKeyFieldName = eventKeyFieldName;
        }

        public async Task AcceptEventAsync(Action<string> eventCallback = null)
        {
            await simpleListener.AcceptClientsAsync(eventData =>
            {
                try
                {
                    JObject eventObj = JObject.Parse(eventData);

                    eventObj.Add("event_id", Guid.NewGuid().ToString("N"));

                    var eventKeyValue = eventObj[eventKeyFieldName].Value<string>();

                    bool isUpdated = cache.TrySetValue(eventKeyValue, (cache, value) =>
                    {
                        List<object> objList = value as List<object>;

                        objList.Add(eventObj);

                        return true;
                    });

                    if (!isUpdated)
                    {
                        List<object> objList = new List<object> { eventObj };
                        cache.Add(eventKeyValue, objList);
                    }

                    eventCallback?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IListener listener_ = simpleListener;
                    simpleListener = null;
                    if (listener_ != null)
                    {
                        listener_.Dispose();
                        listener_ = null;
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