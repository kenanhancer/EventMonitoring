# EventMonitoring

EventMonitoring is a a backend event monitoring application written in .NET Core.

Clients can connect with HTTP connection.

There are two projects in solution. Their names are <b>MonitoringConsoleApp</b> and <b>ClientTestApp</b> respectively.

<b>MonitoringConsoleApp</b> is a event server application. <b>ClientTestApp</b> is a test application as shown below.

![1](https://cloud.githubusercontent.com/assets/1851856/24085344/0d002ade-0d03-11e7-9ec2-8274f97bd78d.PNG)

You can test single threaded or multithreaded. Let's see how to test. 6 clients send random events to server. After that it showed result.

![2](https://cloud.githubusercontent.com/assets/1851856/24085433/ba087e60-0d04-11e7-85c5-2aab8605a8ce.PNG)

There is a simple help menu in Monitoring Server application.

![3](https://cloud.githubusercontent.com/assets/1851856/24085466/76a395c8-0d05-11e7-967e-a4ac4ef79cf0.PNG)

When you write <b>help()</b>, it shows all commands.

![4](https://cloud.githubusercontent.com/assets/1851856/24085467/76a3ca52-0d05-11e7-8ad2-4d0eed043b2a.PNG)

When you write <b>info()</b>, it shows short information.

![5](https://cloud.githubusercontent.com/assets/1851856/24085469/76a53e5a-0d05-11e7-901b-7b598b9225f7.PNG)

if you want to see events of any client, use <b>getclient</b>
![6](https://cloud.githubusercontent.com/assets/1851856/24085468/76a4dd84-0d05-11e7-9007-6c2d39af5bb5.PNG)
