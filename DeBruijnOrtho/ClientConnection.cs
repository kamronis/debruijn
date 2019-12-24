using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace DeBruijn
{
    public class ClientConnection
    {
        TcpClient client;
        Stream stream;
        BinaryReader br;
        BinaryWriter bw;
        private NodesPart storage;
        public ClientConnection(string IP, int port, NodesPart storage)
        {
            client = new TcpClient(IP, port);
            this.storage = storage;
            //stream = client.GetStream();
            stream = new BufferedStream(client.GetStream(), Options.bufferSize);
            br = new BinaryReader(stream);
            bw = new BinaryWriter(stream);

        }
        public bool ReceiveAndExecuteCommand()
        {
            byte comm = br.ReadByte();
            //Console.WriteLine($"ReceiveAndExecuteCommand received command {comm}");
            if (comm == 255) { return false; }
            else if (comm == 0) { bw.Write((byte)4); }
            else if (comm == 1) // IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords)
            {
                // Читаем длину, создаем вектор, читаем вектор
                long len = br.ReadInt64();
                BWord[] arr = new BWord[len];
                for (int i = 0; i < len; i++)
                {
                    arr[i] = BWord.ReadBWord(br);
                }
                // Выполним команду
                var result = storage.GetSetNodes(arr);

                int[] rarr = result.ToArray();
                int cnt = rarr.Length;
                if (cnt != (int)len) throw new Exception("232211");
                // Отправим результат
                bw.Write((long)cnt);
                for (int i = 0; i < len; i++)
                {
                    bw.Write(rarr[i]);
                }
            }
            else if (comm == 2) { storage.DropDictionary(); }
            else if (comm == 3) { storage.MakePrototype(); }
            else if (comm == 4) { storage.Close(); }
            else if (comm == 5) { var cnt = storage.Count(); bw.Write(cnt); }
            //else if (comm == 6) { storage.Restore(); }
            else if (comm == 7)
            {
                int node = br.ReadInt32();
                int prev = br.ReadInt32();
                storage.SetNodePrev(node, prev);
                //bw.Write((byte)77);
            }
            else if (comm == 8)
            {
                int node = br.ReadInt32();
                int next = br.ReadInt32();
                storage.SetNodeNext(node, next);
                //bw.Write((byte)78);
            }
            else if (comm == 9) { storage.Save(); }
            else if (comm == 10)
            {
                int nom = br.ReadInt32();
                LNode lnode = storage.GetLNodeLocal(nom);
                bw.Write(lnode.prev);
                bw.Write(lnode.next);
            }
            else if (comm == 11)
            {
                long nargs = br.ReadInt64();
                int[] codes = new int[nargs];
                for (int i = 0; i< nargs; i++)
                {
                    codes[i] = br.ReadInt32();
                }
                LNode[] resu = storage.GetNodes(codes).ToArray();
                if (resu.Length != codes.Length) throw new Exception("229848");
                bw.Write((long)resu.Length);
                for (int i = 0; i < nargs; i++)
                {
                    LNode cnode = resu[i];
                    //bw.Write(cnode.bword);
                    bw.Write(cnode.prev);
                    bw.Write(cnode.next);
                }
            }
            else if (comm == 12) { bool firsttime = br.ReadBoolean(); storage.Init(firsttime); }
            else if (comm == 14) { storage.RestoreWNodes(); }
            else if (comm == 15) { storage.RestoreInitLNodes(); }
            else if (comm == 16) { storage.RestoreDeactivateWNodes(); }
            else if (comm == 17) { storage.RestoreLNodes(); }
            else if (comm == 18)
            {
                long nargs = br.ReadInt64();
                int[] codes = new int[nargs];
                for (int i = 0; i < nargs; i++)
                {
                    codes[i] = br.ReadInt32();
                }
                BWord[] resu = storage.GetWNodes(codes).ToArray();
                if (resu.Length != codes.Length) throw new Exception("229849");
                bw.Write((long)resu.Length);
                for (int i = 0; i < nargs; i++)
                {
                    BWord cnode = resu[i];
                    BWord.WriteBWord(cnode, bw);
                }
            }

            else throw new Exception("Err: comm=" + comm);
            return true;
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
