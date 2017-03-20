# EventMonitoring

EventMonitoring is a backend event monitoring application written in .NET Core.

Clients can connect with HTTP connection.

There are two projects in solution. Their names are <b>MonitoringConsoleApp</b> and <b>ClientTestApp</b> respectively.

<b>MonitoringConsoleApp</b> is a event server application. <b>ClientTestApp</b> is a test application as shown below.

![2](https://cloud.githubusercontent.com/assets/1851856/24094103/d64e1e94-0d5f-11e7-9373-4170f8a3fa63.PNG)

You can test single threaded or multithreaded. Let's see how to test. 6 clients send random events to server. After that it showed result.

![3](https://cloud.githubusercontent.com/assets/1851856/24094101/d6433150-0d5f-11e7-9be3-2b0ce6ace547.PNG)

There is a simple help menu in Monitoring Server application.

![1](https://cloud.githubusercontent.com/assets/1851856/24094102/d6486710-0d5f-11e7-8c40-3b78c059d317.PNG)

When you write <b>help()</b>, it shows all commands.

![5](https://cloud.githubusercontent.com/assets/1851856/24094098/d623be9c-0d5f-11e7-955a-41eee83ae56b.PNG)

When you write <b>info()</b>, it shows short information.

![9](https://cloud.githubusercontent.com/assets/1851856/24094095/d61e4b2e-0d5f-11e7-94ba-78bc5969bbdc.PNG)

if you want to see events of any client, use <b>getclient</b>
![7](https://cloud.githubusercontent.com/assets/1851856/24094097/d623718a-0d5f-11e7-9ac6-3f4f87a22a5c.PNG)

Let's make 10000 requests concurrently. Request count is 10000. But, Respond count is 4735. it is less. Because, maximum number of concurrent request sended. 
![7](https://cloud.githubusercontent.com/assets/1851856/24085575/4f55dac4-0d07-11e7-9a69-0bac6e42af53.PNG)

When CPU usage reached 100%, Monitoring server app can't take new request. So, request count is 4735 in server app, even though client test app sends 10000 requests. Notice that response count is 4735 above picture. 

![8](https://cloud.githubusercontent.com/assets/1851856/24085678/b7e9cfc2-0d08-11e7-89b4-ea67d4c2d377.PNG)

## How to run applications

First of all, download .NET Core.

Linux(ubuntu)
https://www.microsoft.com/net/core#linuxubuntu

Windows
https://www.microsoft.com/net/core#windowscmd

Download project from github and extract folder. After that, open two terminal and enter PublishOutput directory. Run below commands respectively.

dotnet MonitoringConsoleApp.dll

dotnet ClientTestApp.dll

That is all.

![10](https://cloud.githubusercontent.com/assets/1851856/24094608/23a5071e-0d62-11e7-9a21-d3f1e8a8fa60.PNG)
