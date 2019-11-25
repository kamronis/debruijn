using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeBreinOrtho;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main61(string[] args)
        {
            Console.WriteLine("Start Main61: Extract chains.");

            sw.Restart();

            //NodesPart[] parts = new NodesPart[] { new NodesPart(options.nodelistfilename) };
            //foreach (var part in parts) part.Restore();
            graph.InitParts();
            graph.Restore();

            /// <summary>
            /// Извлечение цепочек
            /// </summary>
            {
                int nchains = 0;
                int maxchain = 0;
                CNode[] maxlist = new CNode[0];

                // Цикл по всем частям и всем узлам
                for (int ipart = 0; ipart < graph.nparts; ipart++)
                {
                    //var part = parts[ipart];
                    //int nodescount = part.Count();
                    int nodescount = graph.PartNodesCount(ipart);
                    for (int n = 0; n < nodescount; n++)
                    {
                        // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
                        // предыдущих несколько или предыдущий один, но у него несколько следующих.
                        // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.

                        //CNode node = nodes[n];

                        CNode ndd = graph.GetNode(graph.ConstructCode(ipart, n));

                        // Проверяем условие стартового узла
                        //if (ndd.prev == -1 ||
                        //    ndd.prev == -2 ||
                        //    ndd.prev >= 0 && nodes[ndd.prev].next == -2
                        //    ) { } // начальный
                        //else { continue; }
                        if (ndd.prev == -1 || ndd.prev == -2) { }
                        else
                        {
                            var ndd1 = graph.GetNode(ndd.prev);
                            if (ndd1.next == -2) { }
                            else continue;
                        }

                        // Зафиксируем новую цепочку
                        List<CNode> list = new List<CNode>(new CNode[] { ndd });
                        // пробежимся вперед
                        CNode nxt;
                        while (HasSingleNext(ndd) && ndd.next >= 0 && HasSinglePrev((nxt = graph.GetNode(ndd.next))))
                        {
                            ndd = nxt;
                            list.Add(ndd);
                        }
                        nchains++;
                        if (list.Count > maxchain) { maxchain = list.Count; maxlist = list.ToArray(); }
                    }
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


            graph.Close();

            sw.Stop();
            Console.WriteLine($"Extract chains ok. duration: {sw.ElapsedMilliseconds}");

        }
    }
}
