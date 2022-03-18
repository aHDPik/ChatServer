using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    internal class Program
    {
        static UdpClient client;
        static IPEndPoint serverEp;
        const int serverPort = 2544;

        static async Task<string> SendAndReceiveCommand(string command)
        {
            byte[] commandDatagram = Encoding.UTF8.GetBytes(command);
            await client.SendAsync(commandDatagram, commandDatagram.Length, serverEp);
            UdpReceiveResult result = await client.ReceiveAsync();
            string reply = Encoding.UTF8.GetString(result.Buffer);
            return reply;
        }


        static async Task Main(string[] args)
        {
            client = new UdpClient();
            serverEp = new IPEndPoint(IPAddress.Loopback, serverPort);
            string command = null;
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            string reply = await SendAndReceiveCommand($"Register;{username}");
            if (reply == "Complete")
                Console.WriteLine("User created");
            else
                Console.WriteLine("Authorized");
            while (command != "exit")
            {
                Console.Write("Enter command: ");
                command = Console.ReadLine();

                if (command != "exit")
                {
                    switch (command)
                    {
                        case "Send":
                            {
                                Console.WriteLine("Enter message: ");
                                string msg = Console.ReadLine();
                                Console.WriteLine("Enter user: ");
                                string receiver = Console.ReadLine();
                                reply = await SendAndReceiveCommand($"Send;{username};{receiver};{msg}");
                                break;
                            }

                        case "Receive":
                            {
                                Console.WriteLine("Enter for how many days: ");
                                int days = int.Parse(Console.ReadLine());
                                reply = await SendAndReceiveCommand($"Receive;{username};{DateTime.Now.AddDays(-days)}");
                                break;
                            }
                    }
                }
                Console.WriteLine(reply);
            }

        }
    }
}
