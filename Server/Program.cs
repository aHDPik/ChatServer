using ChatDB;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static UdpClient server;
        const int serverPort = 2544;
        static bool isRunning;

        static async Task Listen()
        {
            using (ChatContext chatContext = new ChatContext())
            {
                chatContext.Database.EnsureCreated();
                while (isRunning)
                {
                    UdpReceiveResult commandDatagram = await server.ReceiveAsync();
                    string command = Encoding.UTF8.GetString(commandDatagram.Buffer);
                    string[] parts = command.Split(';');
                    bool result = false;
                    string reply = "";
                    StringBuilder sb = new StringBuilder();
                    List<Message> messages = new List<Message>();
                    switch (parts[0])
                    {
                        case "Register":
                            result = await chatContext.RegisterUser(parts[1]);
                            if (!result)
                                reply = "Name already exists";
                            else
                                reply = "Complete";
                            break;
                        case "Send":
                            result = await chatContext.SendMessage(parts[1], parts[2], parts[3]);
                            if (!result)
                                reply = "Could not send message";
                            else
                                reply = "Message sent";
                            break;
                        case "Receive":
                            messages = await chatContext.GetAllMyMessages
                                (parts[1], DateTime.Parse(parts[2]));
                            foreach (Message message in messages)
                            {
                                sb.Append($"{message.Sender.Name}:{message.Text}\n");
                            }
                            reply = sb.ToString();
                            break;
                            
                    }
                    byte[] replyDatagram = Encoding.UTF8.GetBytes(reply);
                    await server.SendAsync(replyDatagram, replyDatagram.Length, commandDatagram.RemoteEndPoint);

                }
            }
        }

        static async Task Main(string[] args)
        {
            server = new UdpClient(serverPort);
            isRunning = true;
            Task listenTask = Listen();
            string command = null;
            while (command != "exit")
                command = Console.ReadLine();
            isRunning = false;
            await listenTask;
        }

    }
}
