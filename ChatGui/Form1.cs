using SocketExtensions;
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

            from = DateTime.Now.AddDays(-2);
        }

        TcpClient client = null;
        NetworkStream stream = null;
        //Убираем ссылки на Reader
        private async void sendButton_Click(object sender, EventArgs e)
        {
            string request = $"Send;{username.Text};{receivername.Text};{message.Text}";
            await stream.WriteLineAsync(request);
            await stream.FlushAsync();
            string reply = await stream.ReadLineAsync();
            if (reply != "Message sent")
            {
                MessageBox.Show("Не удалось отправить сообщение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //chat.Text += $"{receivername.Text}:{message.Text}\r\n";
                if (!timer.Enabled)
                    timer.Start();
            }
        }

        private async void registerButton_Click(object sender, EventArgs e)
        {
            string request = $"Register;{username.Text}";
            await stream.WriteLineAsync(request);
            await stream.FlushAsync();
            string reply = await stream.ReadLineAsync();
            if (reply == "Complete")
                MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Пользователь уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        DateTime from;
        bool timerReady = true;
        private async void timer_Tick(object sender, EventArgs e)
        {
            if (timerReady)
            {
                timerReady = false;
                string request = $"Receive;{username.Text};{from}";
                from = DateTime.Now;
                await stream.WriteLineAsync(request);
                await stream.FlushAsync();
                string reply = await stream.ReadLineAsync();
                if (reply != null)
                    chat.Text += reply;
                timerReady = true;
            }
        }
        //Выносим соединение с сервером на отдельную кнопку, чтобы программа не подключалась автоматически
        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient("localhost", 56234);
                stream = client.GetStream();
                connectButton.Enabled = false;
                sendButton.Enabled = true;
                registerButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сервер не доступен","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
    }
}
