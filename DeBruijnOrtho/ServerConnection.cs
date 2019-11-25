using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace DeBruijnNametable
{
    public class ServerConnection
    {
        private TcpListener listener;
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
        internal BinaryClient[] clients;
        /// <summary>
        /// Запускает цикл подсоединения клиентов. Идет формирование и подсоединение клиента, клиент помещается в массив 
        /// клиентов метод заканчивается когда число подсоединенных клиентов совпадет с заказанным
        /// </summary>
        public void Start(int nclients)
        {
            clients = new BinaryClient[nclients];
            int iclient = 0;
            while (iclient < nclients)
            {
                Console.Write("waiting... ");
                TcpClient client = listener.AcceptTcpClient();
                {
                    Console.WriteLine("connected!");
                    client.NoDelay = true;
                    client.LingerState = new LingerOption(true, 0);

                    clients[iclient] = new BinaryClient(client);
                    iclient++;
                }
            }
        }
        public void Release()
        {
            for (int i = 0; i < clients.Length; i++) clients[i].Release(); 
        }
    }

    public class BinaryClient
    {
        public BinaryReader BReader { get { return br; } }
        public BinaryWriter BWriter { get { return bw; } }
        private TcpClient client;
        private Stream stream;
        private BinaryReader br;
        private BinaryWriter bw;
        public BinaryClient(TcpClient client)
        {
            this.client = client;
            //stream = client.GetStream();
            stream = new BufferedStream(client.GetStream());
            br = new BinaryReader(stream);
            bw = new BinaryWriter(stream);
        }
        public void Release()
        {
            stream.Close();
            client.Close();
        }
    }

}
