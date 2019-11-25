using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCP
{
    class ClientConnection
    {
        TcpClient client;
        NetworkStream stream;
        BinaryReader br;
        BinaryWriter bw;
        public ClientConnection(string IP, int port)
        {
            client = new TcpClient(IP, port);
            stream = client.GetStream();
            br = new BinaryReader(stream);
            bw = new BinaryWriter(stream);

        }

        public byte[] SendReceive(byte[] arr)
        {
            // Посылаю массив
            bw.Write(arr.Length);
            bw.Write(arr); // разобраться с нулевым массивом

            // Принимаю длину от сервера
            int rarr_size = br.ReadInt32();
            byte[] rarr = br.ReadBytes(rarr_size); 
            return rarr;
        }
    }
}
