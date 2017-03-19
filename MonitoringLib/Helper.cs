using System;
using System.Net;
using System.Text;

namespace MonitoringLib
{
    public static class Helper
    {
        public static IPAddress GetIpAdress(string ip)
        {
            IPAddress ipAddress;

            if (!IPAddress.TryParse(ip, out ipAddress))
                throw new ArgumentOutOfRangeException("ip");

            return ipAddress;
        }

        public static int GetPort(string portNumber)
        {
            int port;

            if (!int.TryParse(portNumber, out port) || port < 1024 || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException("port");

            return port;
        }

        public static byte[] GetBodyBuffer(string httpVersion, string mIMEHeader, int iTotBytes, string statusCode, string body)
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(mIMEHeader))
            {
                mIMEHeader = "text/html";  // Default Mime Type is text/html
            }

            sb.AppendLine($"{httpVersion} {statusCode}");
            sb.AppendLine("Server: cx1193719-b");
            sb.AppendLine($"Content-Type: {mIMEHeader}");
            sb.AppendLine("Accept-Ranges: bytes");
            sb.AppendLine($"Content-Length: {iTotBytes}");
            sb.AppendLine();
            sb.AppendLine(body);

            byte[] bodyBuffer = Encoding.UTF8.GetBytes(sb.ToString());

            return bodyBuffer;
        }
    }
}
