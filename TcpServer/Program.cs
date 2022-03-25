using ChatDB;
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

        static void WorkWithClient(TcpClient client)
        {
            using(NetworkStream stream = client.GetStream())
            using (StreamReader sr = new StreamReader(stream))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                string command=null;
                do
                {
                    command = sr.ReadLine();
                    string[] parts = command.Split(';');
                    bool result = false;
                    string reply = "";
                    StringBuilder sb = new StringBuilder();
                    List<Message> messages = new List<Message>();
                    switch (parts[0])
                    {
                        case "Register":
                            result = chatContext.RegisterUser(parts[1]).Result;
                            if (!result)
                                reply = "Name already exists";
                            else
                                reply = "Complete";
                            break;
                        case "Send":
                            result = chatContext.SendMessage(parts[1], parts[2], parts[3]).Result;
                            if (!result)
                                reply = "Could not send message";
                            else
                                reply = "Message sent";
                            break;
                        case "Receive":
                            messages = chatContext.GetAllMyMessages
                                (parts[1], DateTime.Parse(parts[2])).Result;
                            foreach (Message message in messages)
                            {
                                sb.Append($"{message.Sender.Name}:{message.Text}\n");
                            }
                            reply = sb.ToString();
                            break;

                    }
                    sw.WriteLine(reply);
                    stream.Flush();
                } while (command != "Exit");
            }
        }

        static async Task Listen()
        {
            Queue<Thread> connections = new Queue<Thread>();
            while (true)
            {
                TcpClient connection = await listener.AcceptTcpClientAsync();
                Thread t = new Thread(()=>WorkWithClient(connection));
                t.IsBackground = true;
                t.Start();
                connections.Enqueue(t);
            }
        }


        static async Task Main(string[] args)
        {
            listener = new TcpListener(System.Net.IPAddress.Any,56234);
            listener.Start();
            await Listen();
        }
    }
}
