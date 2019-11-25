using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeBreinOrtho;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main6(string[] args)
        {
            Console.WriteLine("Start Main6: Extract chains.");
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //string nodelistfilename = @"D:\Home\data\deBrein\nlist.bin";
            //int nsymbols = 20;

            sw.Restart();

            Stream nodeliststream = File.Open(Options.nodelistfilename, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader brnl = new BinaryReader(nodeliststream);

            // Восстанавливаем список
            List<CNode> nodes = new List<CNode>();
            long nnodes = brnl.ReadInt64();
            for (int i = 0; i < nnodes; i++)
            {
                UInt64 bword = brnl.ReadUInt64();
                int prev = brnl.ReadInt32();
                int next = brnl.ReadInt32();
                nodes.Add(new CNode() { bword = bword, prev = prev, next = next });
            }

            sw.Restart();

            ExtractChains(nodes, nsymbols);

            sw.Stop();
            Console.WriteLine($"Extract chains ok. duration: {sw.ElapsedMilliseconds}");

        }
        // Определим функции: узел не null и у узла есть единичная ссылка назад или вперед 
        static Func<CNode, bool> HasSinglePrev = (CNode nd) => nd.prev != -1 && nd.prev != -2;
        static Func<CNode, bool> HasSingleNext = (CNode nd) => nd.next != -1 && nd.next != -2;

        /// <summary>
        /// Извлечение цепочек
        /// </summary>
        public static void ExtractChains(List<CNode> nodes, int nsymbols)
        {
            int nchains = 0;
            int maxchain = 0;
            CNode[] maxlist = new CNode[0];

            // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
            // предыдущих несколько или предыдущий один, но у него несколько следующих.
            // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.
            for (int n = 0; n < nodes.Count; n++)
            {
                CNode node = nodes[n];
                //if (node.waschecked) continue; -- ОТМЕТКИ НЕ ИГРАЮТ РОЛИ 
                CNode ndd = node;
                // Проверяем условие стартового узла
                if (ndd.prev == -1 ||
                    ndd.prev == -2 ||
                    ndd.prev >= 0 && nodes[ndd.prev].next == -2
                    ) { } // начальный
                else { continue; }

                // Зафиксируем новую цепочку
                List<CNode> list = new List<CNode>(new CNode[] { ndd });
                // пробежимся вперед
                //while (HasSingleNext(ndd) && HasSinglePrev(nodes[ndd.next])) { ndd = nodes[ndd.next]; list.Add(ndd); }
                while (HasSingleNext(ndd) && ndd.next >= 0 && HasSinglePrev(nodes[ndd.next]))
                {
                    ndd = nodes[ndd.next];
                    list.Add(ndd);
                }
                nchains++;
                if (list.Count > maxchain) { maxchain = list.Count; maxlist = list.ToArray(); }
            }
            Console.WriteLine($"==== nchains: {nchains}  maxchain: {maxchain}");

            // Выдача максимальной цепочки
            Console.Write(DBNode.UnCombine(maxlist[0].bword, nsymbols));
            for (int i = 1; i < maxlist.Length; i++)
            {
                CNode node = maxlist[i];
                var word = node.bword;
                string sword = DBNode.UnCombine(word, nsymbols);
                Console.Write(sword[sword.Length - 1]);
            }
            Console.WriteLine();
        }

    }
}
