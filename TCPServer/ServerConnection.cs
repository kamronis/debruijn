using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCP
{
    class ServerConnection
    {
        private TcpListener listener;
        private BinaryReader br;
        private BinaryWriter bw;
        /// <summary>
        /// Устанавливает слушателя на хост и порт
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public ServerConnection(string host, int port)
        {
            IPAddress ipaddr = IPAddress.Parse(host);
            listener = new TcpListener(ipaddr, port);
            listener.Start();
        }
        /// <summary>
        /// Запускает цикл обслуживания клиентов. Идет подсоединение клиента, потом во внотреннем цикле читаем число байтов,
        /// читаем массив с этим числом байтов.
        /// (Пока) посылаем число 4 и сисло принятых байтов
        /// В конце щикла обслуживания, клиент присылает массив с нулевым числом байтов, после этого - вовращаемся в ожидание
        /// появления клиента
        /// </summary>
        public void Start()
        {
            while (true)
            {
                Console.Write("waiting... ");
                using (TcpClient client = listener.AcceptTcpClient())
                {
                    Console.WriteLine("connected!");
                    client.NoDelay = true;
                    client.LingerState = new LingerOption(true, 0);

                    using (NetworkStream stream = client.GetStream())
                    {
                        br = new BinaryReader(stream);
                        bw = new BinaryWriter(stream);
                        while (true)
                        {
                            // Принимаем
                            int nbytes = br.ReadInt32();
                            byte[] bytes = br.ReadBytes(nbytes);
                            if (bytes.Length != nbytes) throw new Exception("bytes.Length != nbytes");

                            // Посылаем
                            // возвращаем длину полученного массива
                            bw.Write(4);
                            bw.Write(nbytes);
                            // ===> выход по нулю байтов в массиве передачи
                            if (nbytes == 0) break; 
                        }
                    }
                }
                //listener.Stop();
            }
        }
    }
}
