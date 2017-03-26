using MonitoringLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTestApp
{
    public class Program
    {
        static int requestCount = 0;

        static int responseCount;

        static bool showEventsToConsole = false;

        static string[] yesArray = { "yes", "y", "1", "true" };
        static string[] noArray = { "no", "n", "0", "false" };

        static string[] eventTypeArray = { "BUSINESS_SERVICE_CALL", "BUSINESS_SERVICE_EXCEPTION", "UNKNOWN", "LOGIN", "LOGOUT", "CHANGE_PASSWORD", "SEND_RESET_KEY" };

        static string[] randomEventValueArray = Enumerable.Range(0, 1000).Select(index => $"Test event {index}").ToArray();

        static string[] randomUserArray = Enumerable.Range(0, 1000).Select(index => Guid.NewGuid().ToString("N")).ToArray();

        static int seed = Environment.TickCount;
        //Thread-safe random
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static void Main(string[] args)
        {
            Console.WriteLine("Event Monitoring Test Application");
            Console.WriteLine("=================================");

            //string ip = "127.0.0.1";
            //string port = "8080";

            //Console.WriteLine($"IP(127.0.0.1): {ip}");
            //Console.WriteLine($"Port(8080): {port}");


#if DEBUG
                string ip = "127.0.0.1";
                string port = "8080";

                Console.WriteLine($"IP(127.0.0.1): {ip}");
                Console.WriteLine($"Port(8080): {port}");
#else
            Console.Write("IP(127.0.0.1): ");
            string ip = Console.ReadLine().Trim();
            IPAddress ipAddress;

            while (String.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out ipAddress))
            {
                Console.Write("IP(127.0.0.1): ");
                ip = Console.ReadLine().Trim();
            }

            Console.Write("Port(8080): ");
            string port = Console.ReadLine().Trim();

            while (String.IsNullOrEmpty(port) || (port.ToCharArray().Any(c => !Char.IsNumber(c))))
            {
                Console.Write("Port(8080): ");
                port = Console.ReadLine().Trim();
            }
#endif

            Func<Task<int>> initalMenu = async () =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;

                    cts.Cancel();
                };

                Console.WriteLine("\nTest Cases:");
                Console.WriteLine("===========");
                Console.WriteLine("1-) Single threaded test");
                Console.WriteLine("2-) Multithreaded test");
                Console.WriteLine("3-) exit");
                Console.WriteLine();
                Console.Write("Enter test case number: ");
                string testNumber = Console.ReadLine().Trim();

                while (String.IsNullOrEmpty(testNumber) || (testNumber != "1" && testNumber != "2" && testNumber != "3"))
                {
                    Console.Write("Enter test case number: ");
                    testNumber = Console.ReadLine().Trim();
                }

                if (testNumber == "3")
                    return await Task.FromResult<int>(1);

                Console.Write("Enter client number: ");
                string clientNumer = Console.ReadLine().Trim();

                while (String.IsNullOrEmpty(clientNumer) || (clientNumer.ToCharArray().Any(c => !Char.IsNumber(c))))
                {
                    Console.Write("Enter client number: ");
                    clientNumer = Console.ReadLine().Trim();
                }

#if DEBUG
                clientNumer = clientNumer.ValueOrNull() ?? "2";
#endif

                Console.Write("Show events(Y/N): ");
                string showEvents = Console.ReadLine().Trim().ToLower();

#if DEBUG
                showEvents = showEvents.ValueOrNull() ?? "Y";
#endif

                while (String.IsNullOrEmpty(showEvents) || (!yesArray.Contains(showEvents) && !noArray.Contains(showEvents)))
                {
                    Console.Write("Show events(Y/N): ");
                    showEvents = Console.ReadLine().Trim();
                }

                Dictionary<string, string> argsDict = new Dictionary<string, string>();
                argsDict.Add("testnumber", testNumber);
                argsDict.Add("ipnumber", ip);
                argsDict.Add("port", port);
                argsDict.Add("clientnumber", clientNumer);
                argsDict.Add("showevents", showEvents);

                int result_ = await MainAsync(argsDict, cts.Token).ContinueWith(t => t.Result, cts.Token).ThrowsAsync(ex => Console.WriteLine(ex.Message));

                return result_;
            };

            int result = 0;
            while (result == 0)
            {
                result = initalMenu().Result;

                Console.WriteLine();
                Console.WriteLine();
            }

            //Console.ReadLine();
        }

        static async Task<int> MainAsync(Dictionary<string, string> argsDict, CancellationToken token)
        {
            if (argsDict == null || argsDict.Count < 4)
                throw new ArgumentOutOfRangeException("argsDict");

            //FileStream filestream = new FileStream("Log.txt", FileMode.Truncate);
            //var streamwriter = new StreamWriter(filestream);
            //streamwriter.AutoFlush = true;

            //Console.SetOut(streamwriter);
            //Console.SetError(streamwriter);

            string ip;
            string portNumber;
            int port;
            string clientNumber;
            int clientCount = 0;
            string testCaseNumber;
            string showEvents;

            if (!argsDict.TryGetValue("ipnumber", out ip))
                throw new ArgumentOutOfRangeException("ipnumber");

            if (!argsDict.TryGetValue("port", out portNumber) || !int.TryParse(portNumber, out port))
                throw new ArgumentOutOfRangeException("port");

            if (!argsDict.TryGetValue("clientnumber", out clientNumber) || !int.TryParse(clientNumber, out clientCount))
                throw new ArgumentOutOfRangeException("clientnumber");

            if (!argsDict.TryGetValue("testnumber", out testCaseNumber))
                throw new ArgumentOutOfRangeException("testnumber");

            if (!argsDict.TryGetValue("showevents", out showEvents))
                throw new ArgumentOutOfRangeException("showevents");

            showEventsToConsole = yesArray.Contains(showEvents);

            requestCount = 0;
            responseCount = 0;

            string url = $"http://{ip}:{portNumber}";

            Stopwatch sw = Stopwatch.StartNew();

            if (testCaseNumber == "1")
            {
                await SingleThreadClientTest(url, clientCount);
            }
            else if (testCaseNumber == "2")
            {
                ParallelQuery<Task> taskList = MultithreadedClientTest(url, clientCount, token);

                await Task.WhenAll(taskList.ToArray()).ContinueWith(t => t, token);
            }
            else
            {
                return await Task.FromResult<int>(0);
            }

            sw.Stop();

            await Console.Out.WriteLineAsync($"\nProcessor Count: {Environment.ProcessorCount}, Maximum number of concurrently executing tasks that will be used to process the query.");

            await Console.Out.WriteLineAsync($"Request Count: {requestCount}");
            await Console.Out.WriteLineAsync($"Response Count: {responseCount}");
            await Console.Out.WriteLineAsync($"Elapsed: {sw.Elapsed}");
            await Console.Out.WriteLineAsync($"Response Count in 1sn: {responseCount / (sw.Elapsed.Seconds <= 0 ? 1 : sw.Elapsed.Seconds)}");

            return await Task.FromResult<int>(0);
        }

        static ParallelQuery<Task> MultithreadedClientTest(string url, int count, CancellationToken token)
        {
            ParallelQuery<Task> taskList = ParallelEnumerable.Range(0, count)
                                                             .WithDegreeOfParallelism(Environment.ProcessorCount)
                                                             .WithCancellation(token)
                                                             .Select(async index =>
                                                             {
                                                                 await SendLog2(url, token).ContinueWith(t => t, token).ThrowsAsync(ex => Console.WriteLine(ex.Message)).ConfigureAwait(false);
                                                             });

            return taskList;
        }

        static async Task SingleThreadClientTest(string url, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await SendLog2(url, CancellationToken.None).ThrowsAsync(ex => Console.WriteLine(ex.Message)).ConfigureAwait(false);
            }
        }

        static async Task SendLog(string url, CancellationToken token)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Proxy = null;
                webRequest.Method = "POST";
                //webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentType = "application/json";

                string clientId = randomUserArray[random.Value.Next(randomUserArray.Length)];
                string eventId = Guid.NewGuid().ToString("N");
                string eventTimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString();
                string eventKey = eventTypeArray[random.Value.Next(eventTypeArray.Length)];
                string eventValue = randomEventValueArray[random.Value.Next(randomEventValueArray.Length)];

                string body = "{" + $"'client_id':'{clientId}','event_timestamp':{eventTimeStamp},'event_type':'CUMULATIVE','event_key':'{eventKey}', 'event_value':'{eventValue}'" + "}";

                byte[] requestBodyBuffer = Encoding.UTF8.GetBytes(body.Trim());

                using (Stream reqStream = await webRequest.GetRequestStreamAsync().ContinueWith(f => f.Result, token).ConfigureAwait(false))
                {
                    await reqStream.WriteAsync(requestBodyBuffer, 0, requestBodyBuffer.Length, token).ConfigureAwait(false);

                    Interlocked.Increment(ref requestCount);
                }

                if (showEventsToConsole)
                    await Console.Out.WriteLineAsync(body).ContinueWith(f => f, token).ConfigureAwait(false);

                WebResponse webResponse = await webRequest.GetResponseAsync().ContinueWith(f => f.Result, token).ConfigureAwait(false);

                Interlocked.Increment(ref responseCount);

                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    string response = await sr.ReadToEndAsync().ContinueWith(f => f.Result, token).ConfigureAwait(false);

#if DEBUG
                    //await Console.Out.WriteLineAsync($"CLIENT: server response is -> {response}");
#endif
                }
            }
            catch (Exception ex)
            {
                //await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

        static async Task SendLog2(string url, CancellationToken token)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                    string clientId = randomUserArray[random.Value.Next(randomUserArray.Length)];
                    string eventId = Guid.NewGuid().ToString("N");
                    string eventTimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString();
                    string eventKey = eventTypeArray[random.Value.Next(eventTypeArray.Length)];
                    string eventValue = randomEventValueArray[random.Value.Next(randomEventValueArray.Length)];

                    string body = "{" + $"'client_id':'{clientId}','event_timestamp':{eventTimeStamp},'event_type':'CUMULATIVE','event_key':'{eventKey}', 'event_value':'{eventValue}'" + "}";

                    byte[] requestBodyBuffer = Encoding.UTF8.GetBytes(body.Trim());

                    var content = new StringContent(body, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Version = Version.Parse("1.1");
                    request.Content = content;

                    Interlocked.Increment(ref requestCount);

                    if (showEventsToConsole)
                        await Console.Out.WriteLineAsync(body).ContinueWith(f => f, token).ConfigureAwait(false);

                    using (HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, content))
                    {
                        Interlocked.Increment(ref responseCount);

                        using (Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync())
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

    }
}