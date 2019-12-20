using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        public static void Main62()
        {
            Console.WriteLine("Start Main62: Extract chains.");

            sw.Restart();

            //NodesPart[] parts = new NodesPart[] { new NodesPart(options.nodelistfilename) };
            //foreach (var part in parts) part.Restore();
            graph.InitParts();
            graph.Restore62();

            // Обработка графа. Извлечение цепочек

            int nchains = 0;
            LNode[] maxlist = new LNode[0];

            // Цикл по всем частям и всем узлам
            for (int ipart = 0; ipart < Options.nparts; ipart++)
            {
                int nodescount = graph.PartNodesCount(ipart);
                Console.WriteLine($"part {ipart}  #nodes {nodescount}");
                // Поиск узлов начал цепочек
                List<LNode> startpoints = new List<LNode>();
                // Накопитель
                List<int> codes = new List<int>();
                for (int n = 0; n < nodescount; n++) // n - локальный код
                {
                    // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
                    // предыдущих несколько или предыдущий один, но у него несколько следующих.
                    // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.

                    if (n % 100_000_000 == 0) Console.Write($"{n / 100_000_000} ");
                    codes.Add(graph.ConstructCode(ipart, n));
                    if (codes.Count >= Options.buffcnodeslength)
                    {
                        LNode[] nodes = graph.GetNodes(codes).ToArray();
                        var nset1 = nodes
                            .Where(nd => nd.prev == -1 || nd.prev == -2);
                        var cset2 = nodes
                            .Where(nd => !(nd.prev == -1 || nd.prev == -2)).Select(nd => nd.prev);
                        var nset3 = graph.GetNodes(cset2)
                            .Where(ndd1 => ndd1.next == -2);
                        startpoints.AddRange(nset1);
                        startpoints.AddRange(nset3);
                        codes = new List<int>();
                    }
                }
                if (codes.Count > 0)
                {
                    LNode[] nodes = graph.GetNodes(codes).ToArray();
                    var nset1 = nodes
                        .Where(nd => nd.prev == -1 || nd.prev == -2);
                    var cset2 = nodes
                        .Where(nd => !(nd.prev == -1 || nd.prev == -2)).Select(nd => nd.prev);
                    var nset3 = graph.GetNodes(cset2)
                        .Where(ndd1 => ndd1.next == -2);
                    startpoints.AddRange(nset1);
                    startpoints.AddRange(nset3);
                    codes = new List<int>();
                }
                Console.WriteLine();
                Console.WriteLine($"chains: {startpoints.Count}");

                // Прохождение цепочек

                int limit = 1000;
                List<List<LNode>> chains = new List<List<LNode>>();
                for (int ind = 0; ind < startpoints.Count; ind++)
                {
                    LNode nd = startpoints[ind];
                    // Добавляем еще одну цепочку если у узла есть следующий
                    if (nd.next >= 0) chains.Add(new List<LNode>(new LNode[] { nd }));
                    // Возможны варианты: мало цепочек или много цепочек
                    // Будем удлинять и выбраковывать цепочки пока их не станет мало
                    while (chains.Count > limit || (ind == startpoints.Count - 1 && chains.Count > 0))
                    {
                        // текущий вектор кодов узлов
                        IEnumerable<int> next_codes = chains.Select(li => li[li.Count - 1].next);
                        // массив следующих узлов
                        LNode[] next = graph.GetNodes(next_codes).ToArray();
                        // Выбраковывание и формирование нового списка
                        List<List<LNode>> chains_next = new List<List<LNode>>();
                        for (int i = 0; i < chains.Count; i++)
                        {
                            List<LNode> chain = chains[i];
                            LNode ndd = chain[chain.Count - 1];
                            LNode nd_candidate = next[i];
                            // последний элемент ndd цепочки обладает свойствами: ndd.next >= 0 (иначе нельзя удлинить)

                            // Кандидат может не подойти если nd_candidate.prev < 0. тогда выводим список из оборота
                            if (nd_candidate.prev < 0) { nchains++; if (chain.Count > maxlist.Length) maxlist = chain.ToArray(); continue; }

                            // кандидат подошел.
                            chain.Add(nd_candidate);

                            //Он может быть последним в цепочке если у него нет следующего, тогда мыцепочку выводим из оборота
                            if (nd_candidate.next < 0) { nchains++; if (chain.Count > maxlist.Length) maxlist = chain.ToArray(); continue; }

                            // Добавляем в новый список
                            chains_next.Add(chain);
                        }
                        // Делаем новый список текущим
                        chains = chains_next;
                    }
                }
            }
            Console.WriteLine($"==== maxchain: {maxlist.Count()}");

            // Выдача максимальной цепочки
            // В цепочке (надеюсь) больше 1 элемента, определяем код нулевого узла по первому, далее строим список кодов всех узлов
            if (Options.toprintcontig)
            {
                List<int> maxcodes = new List<int>();
                maxcodes.Add(maxlist[1].prev);
                for (int i = 0; i < maxlist.Length - 1; i++)
                {
                    maxcodes.Add(maxlist[i].next);
                }

                // (Деактивировать LNodes,) активировать WNodes
                graph.Restore62words();

                ulong[] maxbwords = graph.GetWNodes(maxcodes).Select(wn => wn.bword).ToArray();
                Console.Write(DBNode.UnCombine(maxbwords[0], Options.nsymbols));
                for (int i = 1; i < maxbwords.Length; i++)
                {
                    var word = maxbwords[i];
                    string sword = DBNode.UnCombine(word, Options.nsymbols);
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
