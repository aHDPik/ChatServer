using ChatDB;
using SocketExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    internal class Program
    {
        static TcpListener listener;
        static ChatContext chatContext = new ChatContext();

        static async Task WorkWithClient(TcpClient client)
        {
            using(NetworkStream stream = client.GetStream())
            {
                string command=null;
                do
                {
                    command = await stream.ReadLineAsync();
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
                            messages = await chatContext.GetAllMyInvolvedMessages
                                (parts[1], DateTime.Parse(parts[2]));
                            foreach (Message message in messages)
                            {
                                sb.Append($"{message.Sender.Name}:{message.Text}\r\n");
                            }
                            reply = sb.ToString();
                            break;

                    }
                    await stream.WriteLineAsync(reply);
                    await stream.FlushAsync();
                } while (command != "Exit");
            }
        }

        static async Task Listen()
        {
            Queue<Task> connections = new Queue<Task>();
            while (true)
            {
                TcpClient connection = await listener.AcceptTcpClientAsync();
                connections.Enqueue(WorkWithClient(connection));
            }
        }


        static async Task Main(string[] args)
        {
            await chatContext.Database.EnsureCreatedAsync();
            listener = new TcpListener(System.Net.IPAddress.Any,56234);
            listener.Start();
            await Listen();
        }
    }
}
