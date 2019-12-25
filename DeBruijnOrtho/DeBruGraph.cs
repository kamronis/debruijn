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
        public void InitParts(bool firsttime = false) 
        {
            parts = new INodePart[Options.nparts];
            // Если число частей (степень двойки) совпадает с числом клиентов, то там части и располагаются, иначе нулевая часть помещается на мастере  
            if (Options.nparts == Options.nslaves)
            {
                for (int i = 0; i < Options.nparts; i++) parts[i] = new NodesPartNet(sconnection.clients[i]);
            }
            else
            {
                parts[0] = new NodesPart(Options.wnodesfilename, Options.lnodesfilename);
                for (int i = 1; i < Options.nparts; i++) parts[i] = new NodesPartNet(sconnection.clients[i - 1]);
            }

            foreach (var part in parts) { part.Init(firsttime); }
        }
        public void Restore51()
        {
            // Восстанавливаем wnodes, инициируем lnodes, деактивируем wnodes
            foreach (var part in parts) 
            { 
                //part.RestoreWNodes(); //TODO: Читать все не нужно-БЫ!
                part.RestoreInitLNodes();
                part.RestoreDeactivateWNodes();
            }
        }
        public void Restore62()
        {
            // Восстанавливаем lnodes
            foreach (var part in parts) { part.RestoreLNodes(); }
        }
        public void Restore62words()
        {
            // Можно деактивировать LNodes
            //foreach (var part in parts) { part.RestoreDeactivateLNodes(); }
            
            // Восстанавливаем wnodes
            foreach (var part in parts) { part.RestoreWNodes(); }
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
        public IEnumerable<int> GetSetNodes(IEnumerable<BWord> bwords)
        {
            //return bwords.Select(w => GetSetNode(w));
            int mask = (Options.nparts - 1);
            BWord[] arr = bwords.ToArray();

            // Разбить массив аргументов по секциям (частям)
            List<BWord>[] wordsbysections = Enumerable.Repeat(1, Options.nparts).Select(w => new List<BWord>()).ToArray(); 
            // Распределим
            for (int i = 0; i < arr.Length; i++)
            {
                BWord bword = arr[i];
                int ipart = (int)(bword.uword & (uint)mask); // bword.Lay; //???
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
                BWord bword = arr[i];
                //int ipart = bword.Lay; //(int)(bword & mask);
                int ipart = (int)(bword.uword & (uint)mask);
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
        internal LNode GetNode(int code)
        {
            int ipart = code & (Options.nparts - 1);
            return parts[ipart].GetLNodeLocal(code >> Options.nshift);
        }
        internal IEnumerable<LNode> GetNodes(IEnumerable<int> codes)
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
            List<LNode>[] cnodesbysections = Enumerable.Repeat(1, Options.nparts).Select(i => new List<LNode>()).ToArray();
            for (int j = 0; j < Options.nparts; j++)
            {
                cnodesbysections[j] = parts[j].GetNodes(codesbysections[j]).ToList();
            }

            // Объединим результаты
            int[] nextind = Enumerable.Repeat(0, Options.nparts).ToArray();
            LNode[] results = new LNode[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                int code = arr[i];
                int ipart = (int)(code & mask);
                LNode lnode = cnodesbysections[ipart][nextind[ipart]];
                results[i] = lnode;
                nextind[ipart] += 1;
            }
            return results;
        }


        internal IEnumerable<BWord> GetWNodes(IEnumerable<int> codes)
        {
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
            List<BWord>[] wnodesbysections = Enumerable.Repeat(1, Options.nparts).Select(i => new List<BWord>()).ToArray();
            for (int j = 0; j < Options.nparts; j++)
            {
                wnodesbysections[j] = parts[j].GetWNodes(codesbysections[j]).ToList();
            }

            // Объединим результаты
            int[] nextind = Enumerable.Repeat(0, Options.nparts).ToArray();
            BWord[] results = new BWord[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                int code = arr[i];
                int ipart = (int)(code & mask);
                BWord lnode = wnodesbysections[ipart][nextind[ipart]];
                results[i] = lnode;
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
