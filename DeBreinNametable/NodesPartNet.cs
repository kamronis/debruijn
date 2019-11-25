using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeBruijnNametable
{
    // Переходник к удаленной части
    public class NodesPartNet : INodePart
    {
        private BinaryClient bclient;
        public NodesPartNet(BinaryClient bclient) { this.bclient = bclient; }
        // Команды: 

        public IEnumerable<int> GetSetNodes(IEnumerable<ulong> bwords) // 1
        {
            // Послать команду 1
            bclient.BWriter.Write((byte)1);
            // Послать количество слов
            var arr = bwords.ToArray();
            bclient.BWriter.Write((long)arr.Length);
            // Послать массив длинных без знака
            for (int i = 0; i < arr.Length; i++)
            {
                bclient.BWriter.Write((UInt64)arr[i]);
            }
            // Принять массив целых
            long n = bclient.BReader.ReadInt64();
            if (n != arr.Length) throw new Exception("287443");
            int[] narr = new int[n];
            for (int i = 0; i < n; i++)
            {
                narr[i] = bclient.BReader.ReadInt32();
            }
            return narr;
        }

        public void DropDictionary() // 2
        {
            bclient.BWriter.Write((byte)2);
        }
        public void MakePrototype() // 3
        {
            bclient.BWriter.Write((byte)3);
        }
        public void Close() // 4
        {
            bclient.BWriter.Write((byte)4);
        }
        public int Count() // 5
        {
            bclient.BWriter.Write((byte)5);
            var res = bclient.BReader.ReadInt32();
            return res;
        }
        public void Restore() // 6
        {
            bclient.BWriter.Write((byte)6);
        }
        public void SetNodePrev(int node, int prevlink) // 7
        {
            bclient.BWriter.Write((byte)7);
            bclient.BWriter.Write(node);
            bclient.BWriter.Write(prevlink);
            //byte r = bclient.BReader.ReadByte();
            //if (r != 77) throw new Exception("Error 77: r=" + r);
        }
        public void SetNodeNext(int node, int nextlink) // 8
        {
            bclient.BWriter.Write((byte)8);
            bclient.BWriter.Write(node);
            bclient.BWriter.Write(nextlink);
            //byte r = bclient.BReader.ReadByte();
            //if (r != 78) throw new Exception("Error 78: r=" + r);
        }

        public void Save() // 9
        {
            bclient.BWriter.Write((byte)9);
        }

        public CNode GetNodeLocal(int nom) // 10
        {
            bclient.BWriter.Write((byte)10);
            bclient.BWriter.Write(nom);
            UInt64 bword = bclient.BReader.ReadUInt64();
            int prev = bclient.BReader.ReadInt32();
            int next = bclient.BReader.ReadInt32();
            return new CNode() { bword = bword, prev = prev, next = next };
        }

        public IEnumerable<CNode> GetNodes(IEnumerable<int> codes) // 11
        {
            bclient.BWriter.Write((byte)11);
            int[] arr = codes.ToArray();
            // Посылаем
            bclient.BWriter.Write((long)arr.Length);
            for (int i = 0; i< arr.Length; i++)
            {
                bclient.BWriter.Write(arr[i]);
            }
            // Принимаем
            int nres = (int)bclient.BReader.ReadInt64();
            if (nres != arr.Length) throw new Exception("2929333");
            CNode[] carr = new CNode[nres];
            for (int i = 0; i< nres; i++)
            {
                UInt64 bword = bclient.BReader.ReadUInt64();
                int prev = bclient.BReader.ReadInt32();
                int next = bclient.BReader.ReadInt32();
                carr[i] = new CNode() { bword = bword, prev = prev, next = next };
            }
            return carr;
        }
        public void Init() // 12
        {
            bclient.BWriter.Write((byte)12);
        }

        public int GetSetNode(ulong bword) // 13
        {
            throw new NotImplementedException();
        }

    }
}
