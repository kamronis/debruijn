using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace DeBreinData
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
        
        public Byte[] SendReceive(Byte[] arr)
        {
            bw.Write(arr.Length);
            stream.Write(arr, 0, arr.Length);

            int inc_arr_size = br.ReadInt32();
            Byte[] inc_arr = new Byte[inc_arr_size];
            int offset = 0;
            while (offset != inc_arr_size)
            {
                
                int curr_size = stream.Read(inc_arr, offset, inc_arr.Length - offset);

                offset += curr_size;
            }
            return inc_arr;

        }
    }
}
