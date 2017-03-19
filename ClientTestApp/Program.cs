using MonitoringLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
            Func<Task<int>> initalMenu = async () =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;

                    cts.Cancel();
                };

                Console.WriteLine("Test Cases:");
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

                Console.Write("Show events: ");
                string showEvents = Console.ReadLine().Trim().ToLower();

                while (String.IsNullOrEmpty(showEvents) || (!yesArray.Contains(showEvents) && !noArray.Contains(showEvents)))
                {
                    Console.Write("Show events: ");
                    showEvents = Console.ReadLine().Trim();
                }

                Dictionary<string, string> argsDict = new Dictionary<string, string>();
                argsDict.Add("testnumber", testNumber);
                argsDict.Add("ipnumber", "127.0.0.1");
                argsDict.Add("port", "8080");
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

            Stopwatch sw = Stopwatch.StartNew();

            if (testCaseNumber == "1")
            {
                await SingleThreadClientTest(clientCount);
            }
            else if (testCaseNumber == "2")
            {
                ParallelQuery<Task> taskList = MultithreadedClientTest(clientCount, token);

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

        static ParallelQuery<Task> MultithreadedClientTest(int count, CancellationToken token)
        {
            ParallelQuery<Task> taskList = ParallelEnumerable.Range(0, count)
                                                             .WithDegreeOfParallelism(Environment.ProcessorCount)
                                                             .WithCancellation(token)
                                                             .Select(async index =>
                                                             {
                                                                 await SendLog(token).ContinueWith(t => t, token).ThrowsAsync(ex => Console.WriteLine(ex.Message));
                                                             });

            return taskList;
        }

        static async Task SingleThreadClientTest(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await SendLog(CancellationToken.None).ThrowsAsync();
            }
        }

        static async Task SendLog(CancellationToken token)
        {
            //Thread.Sleep(1);

            WebRequest webRequest = WebRequest.Create("http://127.0.0.1:8080");
            webRequest.Method = "POST";

            string clientId = randomUserArray[random.Value.Next(randomUserArray.Length)];
            string eventId = Guid.NewGuid().ToString("N");
            string eventTimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString();
            string eventKey = eventTypeArray[random.Value.Next(eventTypeArray.Length)];
            string eventValue = randomEventValueArray[random.Value.Next(randomEventValueArray.Length)];

            //string body = "{" + $"'client_id':'{clientId}','event_id':'{eventId}','event_timestamp':{eventTimeStamp},'event_type':'CUMULATIVE','event_key':'{eventKey}', 'event_value':'{eventValue}'" + "}";
            string body = "{" + $"'client_id':'{clientId}','event_timestamp':{eventTimeStamp},'event_type':'CUMULATIVE','event_key':'{eventKey}', 'event_value':'{eventValue}'" + "}";

            byte[] requestBodyBuffer = Encoding.UTF8.GetBytes(body.Trim());

            //webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentType = "application/json";

            using (Stream reqStream = await webRequest.GetRequestStreamAsync().ContinueWith(t => t.Result, token))
            {
                await reqStream.WriteAsync(requestBodyBuffer, 0, requestBodyBuffer.Length).ContinueWith(t => t, token);

                Interlocked.Increment(ref requestCount);
            }

            if (showEventsToConsole)
                await Console.Out.WriteLineAsync(body);

            WebResponse webResponse = await webRequest.GetResponseAsync().ContinueWith(t => t.Result, token);

            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
            {
                string response = await sr.ReadToEndAsync().ContinueWith(t => t.Result, token);

                Interlocked.Increment(ref responseCount);

#if DEBUG
                //await Console.Out.WriteLineAsync($"CLIENT: server response is -> {response}");
#endif
            }
        }
    }
}