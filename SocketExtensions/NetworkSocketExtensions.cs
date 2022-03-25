using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketExtensions
{/// <summary>
/// Расширение для отправки строк через NetworkStream, так как StreamReader StreamWriter не подходят для этой задачи
/// </summary>
    public static class NetworkSocketExtensions
    {
        /// <summary>
        /// Получение текста через NetworkStream
        /// </summary>
        /// <param name="stream">Поток, из которого надо считать строку</param>
        /// <returns>Считанная строка</returns>
        public static async Task<string> ReadLineAsync(this NetworkStream stream)
        {
            //выделяем 4 байта под размер строки
            byte[] sizeBytes = new byte[sizeof(int)];
            //считываем размер
            await stream.ReadAsync(sizeBytes, 0, sizeof(int));
            //конвертируем байты в число
            int size = BitConverter.ToInt32(sizeBytes);
            if (size > 0)
            {
                //выделяем байты под строку
                byte[] textBytes = new byte[size];
                //считываем строку
                await stream.ReadAsync(textBytes, 0, size);
                //конвертируем байты в строку
                return Encoding.UTF8.GetString(textBytes);
            }
            else
                return null;
        }
        /// <summary>
        /// Отправка текста через NetworkStream
        /// </summary>
        /// <param name="stream">Поток, в который надо положить строку</param>
        /// <param name="message">Сообщение для отправки</param>
        /// <returns></returns>
        public static async Task WriteLineAsync(this NetworkStream stream, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                //Конвертируем строку в байты
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                //Отправляем размер строки в байтах
                await stream.WriteAsync(BitConverter.GetBytes(messageBytes.Length), 0, sizeof(int));
                //Отправляем байты строки
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            else
            {
                await stream.WriteAsync(BitConverter.GetBytes(0), 0, sizeof(int));
            }
        }
    }
}
