using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeBruijn
{
    public class DeBruGraph
    {

        // Граф
        private INodePart[] parts = null;
        private ServerConnection sconnection;
        public DeBruGraph(ServerConnection sconnection)
        {
            this.sconnection = sconnection;    
        }
        public void InitParts() 
        {
            parts = new INodePart[Options.nparts];
            // Если число частей (степень двойки) совпадает с числом клиентов, то там части и распологаются, иначе нулевая часть помещается на мастере  
            if (Options.nparts == Options.nslaves)
            {
                for (int i = 0; i < Options.nparts; i++) parts[i] = new NodesPartNet(sconnection.clients[i]);
            }
            else
            {
                parts[0] = new NodesPart(Options.masterlistfilename);
                for (int i = 1; i < Options.nparts; i++) parts[i] = new NodesPartNet(sconnection.clients[i - 1]);
            }

            foreach (var part in parts) { part.Init(); }
        }
        public void Restore()
        {
            foreach (var part in parts) { part.Restore(); }
        }
        public void Save()
        {
            foreach (var part in parts) { part.Save(); }
        }
        public void Close()
        {
            foreach (var part in parts) part.Close();
        }
        public void DropDictionary()
        {
            foreach (var part in parts) { part.DropDictionary(); }
        }
        public int GetSetNode(UInt64 bword)
        {
            UInt64 mask = (UInt64)(Options.nparts - 1);
            int ipart = (int)(bword & mask);
            int localcode = parts[ipart].GetSetNode(bword);
            return (localcode << Options.nshift) | ipart;
        }
        public IEnumerable<int> GetSetNodes(IEnumerable<UInt64> bwords)
        {
            //return bwords.Select(w => GetSetNode(w));
            UInt64 mask = (UInt64)(Options.nparts - 1);
            UInt64[] arr = bwords.ToArray();

            // Разбить массив аргументов по секциям (частям)
            List<UInt64>[] wordsbysections = Enumerable.Repeat(1, Options.nparts).Select(w => new List<UInt64>()).ToArray(); 
            // Распределим
            for (int i = 0; i < arr.Length; i++)
            {
                UInt64 bword = arr[i];
                int ipart = (int)(bword & mask);
                wordsbysections[ipart].Add(bword);
            }

            // Сделаем запросы к секциям
            List<int>[] codesbysections = Enumerable.Repeat(1, Options.nparts).Select(w => new List<int>()).ToArray();
            for (int j=0; j < Options.nparts; j++)
            {
                codesbysections[j] = parts[j].GetSetNodes(wordsbysections[j]).ToList();
            }

            // Объединим результаты
            int[] nextind = Enumerable.Repeat(0, Options.nparts).ToArray();
            int[] results = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                UInt64 bword = arr[i];
                int ipart = (int)(bword & mask);
                int localcode = codesbysections[ipart][nextind[ipart]];
                nextind[ipart] += 1;
                results[i] = (localcode << Options.nshift) | ipart;
            }
            return results;
        }
        public void MakePrototype()
        {
            for (int ipart = 0; ipart < Options.nparts; ipart++)
            {
                parts[ipart].MakePrototype();
            }
        }
        public void SetNodePrev(int code, int link)
        {
            int ipart = code & (Options.nparts - 1);
            parts[ipart].SetNodePrev(code >> Options.nshift, link);
        }
        public void SetNodeNext(int code, int link)
        {
            int ipart = code & (Options.nparts - 1);
            parts[ipart].SetNodeNext(code >> Options.nshift, link);
        }
        public int PartNodesCount(int ipart)
        {
            return parts[ipart].Count();
        }
        internal CNode GetNode(int code)
        {
            int ipart = code & (Options.nparts - 1);
            return parts[ipart].GetNodeLocal(code >> Options.nshift);
        }
        internal IEnumerable<CNode> GetNodes(IEnumerable<int> codes)
        {
            //return codes.Select(c => GetNode(c));
            int mask = (int)(Options.nparts - 1);
            int[] arr = codes.ToArray();

            // Разбить массив аргументов по секциям (частям)
            List<int>[] codesbysections = Enumerable.Repeat(1, Options.nparts).Select(w => new List<int>()).ToArray();
            // Распределим
            for (int i = 0; i < arr.Length; i++)
            {
                int code = arr[i];
                int ipart = (int)(code & mask);
                codesbysections[ipart].Add(code);
            }

            // Сделаем запросы к секциям
            List<CNode>[] cnodesbysections = Enumerable.Repeat(1, Options.nparts).Select(i => new List<CNode>()).ToArray();
            for (int j = 0; j < Options.nparts; j++)
            {
                cnodesbysections[j] = parts[j].GetNodes(codesbysections[j]).ToList();
            }

            // Объединим результаты
            int[] nextind = Enumerable.Repeat(0, Options.nparts).ToArray();
            CNode[] results = new CNode[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                int code = arr[i];
                int ipart = (int)(code & mask);
                CNode cnode = cnodesbysections[ipart][nextind[ipart]];
                results[i] = cnode;
                nextind[ipart] += 1;
            }
            return results;
        }
        internal int ConstructCode(int part, int localcode)
        {
            return (localcode << Options.nshift) | part;
        }

    }
}
