using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            client = new TcpClient("localhost", 56234);
            stream = client.GetStream();
            sr = new StreamReader(stream);
            sw = new StreamWriter(stream);
            from = DateTime.Now.AddDays(-2);
        }

        TcpClient client = null;
        NetworkStream stream = null;
        StreamReader sr = null;
        StreamWriter sw = null;

        private async void sendButton_Click(object sender, EventArgs e)
        {
            string request = $"Send;{username.Text};{receivername.Text};{message.Text}";
            await sw.WriteLineAsync(request);
            await sw.FlushAsync();
            string reply = await sr.ReadLineAsync();
            if (reply != "Message sent")
            {
                MessageBox.Show("Не удалось отправить сообщение");
            }
            else
            {
                chat.Text += $"{receivername.Text}:{message.Text}";
                if (!timer.Enabled)
                    timer.Start();
            }
        }

        private async void registerButton_Click(object sender, EventArgs e)
        {
            string request = $"Register;{username.Text}";
            await sw.WriteLineAsync(request);
            await stream.FlushAsync();
            string reply = await sr.ReadLineAsync();
            if (reply == "Complete")
                MessageBox.Show("Регистрация прошла успешно!");
            else
                MessageBox.Show("Пользователь уже существует", "Ошибка");
        }

        DateTime from;

        private async void timer_Tick(object sender, EventArgs e)
        {
            string request = $"Receive;{username.Text};{from}";
            from = DateTime.Now;
            await sw.WriteLineAsync(request);
            await stream.FlushAsync();
            string reply = await sr.ReadLineAsync();
            chat.Text += reply;
        }
    }
}
