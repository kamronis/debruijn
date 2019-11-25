using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DeBrein
{
    // Есть одна ссылка на предшественника и одна - на последователя
    // если появляется другая, то устанавливается служебная ссылка на внешний узел dummy
    public class NodeInfo
    {
        private string _code;
        public NodeInfo(string code) { _code = code;  Nprev = null; Nnext = null; waschecked = false; }
        public NodeInfo Clone()
        {
            NodeInfo ni = this;
            return new NodeInfo(ni.Code) { Nprev = ni.Nprev, Nnext = ni.Nnext, waschecked = ni.waschecked };
        }
        public string Code { get { return _code; } }
        public NodeInfo Nprev { get; set; }
        public NodeInfo Nnext { get; set; }
        public bool waschecked { get; set; } 
    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Hello de Brein!");

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 
            // Есть рабочие данные и есть маленький файл с графом деБрейна
            string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            int nsymbols = 20;
            //string readsfilename = @"D:\Home\dev2019\experiments\DeBrein\DeBrein\reads7x3.txt";
            //int nsymbols = 3;

            // Множество узлов выстраивается в хеш-таблицу с ключем в виде кода
            Dictionary<string, NodeInfo> nodes = new Dictionary<string, NodeInfo>();
            // Служебный узел, ссылка на который устанавливается, если число ссылок "назад" или "вперед" больше одной
            NodeInfo dummy = new NodeInfo("dummy");

            sw.Start(); // запускаем секундомер
            // Сканируем данные, вычисляем узлы
            using (TextReader reader = new StreamReader(File.Open(readsfilename, FileMode.Open, FileAccess.Read)))
            {
                string line = null;
                int lcount = 0;
                int nwords = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    int len = line.Length;
                    lcount++;
                    NodeInfo previous = null;
                    for (int i = 0; i< len - nsymbols + 1; i++)
                    {
                        // формируем слово
                        string word = line.Substring(i, nsymbols);
                        nwords++;
                        // находим или создаем текущий узел
                        NodeInfo current = null;
                        if (nodes.TryGetValue(word, out current)) {  }
                        else
                        {
                            current = new NodeInfo(word);
                            nodes.Add(word, current);
                        }
                        if (previous != null)
                        {
                            // добавляем дугу
                            // дуга добавляется если нет ничего, а разрушается если дуга (ссылка) есть и существующая ссылка дугая 
                            current.Nprev = current.Nprev == null ? previous : (current.Nprev == previous ? previous : dummy);
                            previous.Nnext = previous.Nnext == null ? current : (previous.Nnext == current ? current : dummy);
                        }
                        // переходим на следующую итерацию 
                        previous = current;
                    }
                }
                Console.WriteLine($"lines:{lcount} words: {nwords} different: {nodes.Count}");
            }
            sw.Stop(); Console.WriteLine($"Build nodes ok. Duration={sw.ElapsedMilliseconds}");

            // Пройдемся по узлам, будем выявлять транзитные цепочки (континги?)

            // Определим функции: узел не null и у узла есть единичная ссылка назад или вперед 
            Func<NodeInfo, bool> HasSinglePrev = (NodeInfo nd) => nd != null && nd.Nprev != null && nd.Nprev != dummy;
            Func<NodeInfo, bool> HasSingleNext = (NodeInfo nd) => nd != null && nd.Nnext != null && nd.Nnext != dummy;

            sw.Restart();
            List <List<NodeInfo>> chains = new List<List<NodeInfo>>();
            foreach (NodeInfo nd in nodes.Values)
            {
                // Пропускаем уже обработанные
                if (nd.waschecked) continue;
                // Двигаемся назад пока у узла есть один предыдущий и у предыдущего есть один следующий 
                // и мы не вышли на узел с которого начали 
                NodeInfo ndd = nd;
                while (HasSinglePrev(ndd) && HasSingleNext(ndd.Nprev) && !(ndd.Nprev == nd)) { ndd = ndd.Nprev; }

                // Отмечаем, что мы этот узел уже обработали
                ndd.waschecked = true;
                // Если мы остались на начальном узле и у него нет единственного следующего, то цепочки нет
                if (ndd == nd && !HasSingleNext(nd)) continue;

                // Зафиксируем новую цепочку
                List<NodeInfo> list = new List<NodeInfo>(new NodeInfo[] { ndd });
                // пробежимся вперед
                while (HasSingleNext(ndd) && HasSinglePrev(ndd.Nnext)) { ndd = ndd.Nnext; if (ndd.waschecked) break; ndd.waschecked = true; list.Add(ndd); }
                chains.Add(list);
            }
            Console.WriteLine($"{chains.Count} chains.");
            sw.Stop(); Console.WriteLine($"Build chains ok. Duration={sw.ElapsedMilliseconds}");

            //// Длины цепочек
            //foreach (var li in chains) if (li.Count > 79) Console.Write($"{li.Count} ");
            //Console.WriteLine();

            // Анализ цепочек
            int nchains = chains.Count;
            int maxchain = chains.Max(lis => lis.Count);
            Console.WriteLine($"nchains={nchains}  maxchain={maxchain}");

            //// Выдача цепочек
            //foreach (var lis in chains)
            //{
            //    Console.Write(lis[0].Code);
            //    for (int i=1; i<lis.Count; i++)
            //    {
            //        string word = lis[i].Code;
            //        Console.Write(word[word.Length - 1]);
            //    }
            //    Console.WriteLine($" {nsymbols + lis.Count - 1}");
            //}

            // Выдача максимальной цепочки
            var maxlist = chains.First(lis => lis.Count == maxchain);
            Console.Write(maxlist[0].Code);
            for (int i = 1; i < maxlist.Count; i++)
            {
                NodeInfo nd = maxlist[i];
                string word = nd.Code;
                Console.Write(word[word.Length - 1]);
            }
            Console.WriteLine();

            sw.Stop();
            Console.WriteLine($"duration={sw.ElapsedMilliseconds}");

        }
    }
}
