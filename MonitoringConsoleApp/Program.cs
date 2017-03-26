using MonitoringLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Event Monitoring Server Application");
            Console.WriteLine("===================================");

            if (args.Length == 0)
            {
                //string port = "8080";

#if DEBUG
                string port = "8080";

#else   
                Console.Write("Port(8080): ");
                string port = Console.ReadLine().Trim();

                while (String.IsNullOrEmpty(port) || (port.ToCharArray().Any(c => !Char.IsNumber(c))))
                {
                    Console.Write("Port(8080): ");
                    port = Console.ReadLine().Trim();
                }
#endif

                args = new string[] { port };
            }

            MainAsync(args).ThrowsAsync(ex => Console.WriteLine(ex.Message)).Wait();
        }

        static async Task MainAsync(params string[] args)
        {
            if (args == null || args.Length < 1)
                throw new ArgumentOutOfRangeException("args");

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;

                cts.Cancel();
            };

            string ip = "127.0.0.1";

            string portNumber = args[0];

            ICache inMemoryCache = new InMemoryCache();

            SimpleMonitoringServer simpleMonitoringServer = null;

            AutoResetEvent are = new AutoResetEvent(false);

            Task.Run(async () =>
            {
                using (simpleMonitoringServer = new SimpleMonitoringServer(ip, portNumber, inMemoryCache, "client_id", cts.Token))
                {
                    Console.WriteLine($"\nIP: {ip}\nPort: {portNumber}\n");
                    are.Set();

                    await simpleMonitoringServer.AcceptEventAsync(async eventData =>
                    {
                        //await Console.Out.WriteLineAsync(eventData);
                    });
                }
            });

            are.WaitOne();

            string command;
            string commandName;
            string commandValue = String.Empty;
            while (!cts.IsCancellationRequested)
            {

                Console.Write("> ");
                command = Console.ReadLine();

                command = command.Trim().ToLower();

                int first = command.IndexOf('(');
                int last = command.LastIndexOf(')');

                if (first <= 0 || last <= 0)
                    continue;

                commandName = command.Substring(0, first);
                first++;

                if (first != last)
                    commandValue = command.Substring(first, last - first);

                if (commandName == "info")
                {
                    Console.WriteLine($"\nReceived request count: {simpleMonitoringServer?.ReceivedRequestCount}");
                    Console.WriteLine($"\nUnique client count: {inMemoryCache.Count()}");
                }
                else if (commandName == "help")
                {
                    Console.WriteLine("\ngetclient(@clientid)");
                    Console.WriteLine("info()");
                    Console.WriteLine("help()");
                    Console.WriteLine("exit()");
                }
                else if (commandName == "exit")
                {
                    cts.Cancel();
                }
                else if (commandName == "getclient")
                {
                    List<JObject> eventData = inMemoryCache.Get(commandValue).Cast<JObject>().ToList();
                    if (eventData != null)
                    {
                        Console.WriteLine();
                        foreach (var item in eventData.OrderBy(f => f["event_key"]))
                            Console.WriteLine(item.ToString());
                    }

                }

                Console.WriteLine();
            }

            //Console.ReadLine();
        }
    }
}