using System;
using System.Collections.Generic;
using System.IO;

namespace DeBruijnDirect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start DeBruijnDirect.");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 
            // Есть рабочие данные и есть маленький файл с графом деБрейна
            string readsfilename = @"D:\Home\data\deBruijn\reads.txt";
            int nsymbols = 20;
            int npasses = 2;

            sw.Start(); // запускаем секундомер

            // Две важнейших структуры и одна временная
            List<ulong> ccodes = new List<ulong>();
            List<int[]> creads = new List<int[]>();
            Dictionary<ulong, int> dic = new Dictionary<ulong, int>();
            int nnodes = 0;

            FileStream fs = File.Open(readsfilename, FileMode.Open, FileAccess.Read);
            TextReader reader = new StreamReader(fs);

            // Сканируем данные, вычисляем узлы
            for (int ipass = 0; ipass < npasses; ipass++)
            {
                string line;
                int lcount = 0;
                int nwords = 0;
                fs.Position = 0L;
                //for (int iline = 0; iline < lines.Count; iline++)
                while ((line = reader.ReadLine()) != null)
                {
                    //line = lines[iline];
                    int len = line.Length;

                    // создадим или прочитаем кодированный рид
                    int[] reed = null;
                    if (ipass == 0)
                    {
                        reed = new int[len - nsymbols + 1];
                        creads.Add(reed);
                    }
                    else
                    {
                        reed = creads[lcount];
                    }
                    lcount++;

                    for (int i = 0; i < len - nsymbols + 1; i++)
                    {
                        // формируем слово
                        string word = line.Substring(i, nsymbols);
                        nwords++;
                        // Переводим слово в бинарный вид
                        ulong bword = Combine(word);
                        // находим или создаем текущий узел
                        if ((int)(bword & (ulong)(npasses-1)) == ipass)
                        {
                            int code;
                            if (dic.TryGetValue(bword, out code)) { }
                            else
                            {
                                code = nnodes;
                                //ccodes.Add(bword);
                                dic.Add(bword, code);
                                nnodes++;
                            }
                            reed[i] = code;
                        }
                    }
                }
                if (ipass == npasses - 1)
                {
                    Console.WriteLine($"memory used after dictionaries: {GC.GetTotalMemory(false)}");
                    Console.WriteLine($"lines:{lcount} words: {nwords} codes: {nnodes} dictionary : {dic.Count} elements");
                }
                // Теперь нам словарь не поднадобится
                dic = new Dictionary<ulong, int>();
                GC.Collect();
            }

            // Теперь нам нужны узлы со ссылками (номерами) prev и next
            PrevNext[] lnodes = new PrevNext[nnodes];
            for (int i=0; i<nnodes; i++) { lnodes[i].prev = -1; lnodes[i].next = -1; }

            // Сканируем новые кодированные риды
            foreach (var cread in creads)
            {
                // код предыдущего узла
                int previous = -1;
                for (int i = 0; i < cread.Length; i++)
                {
                    // код текущего узла
                    int current = cread[i];
                    if (previous != -1)
                    {
                        // дуга добавляется если нет ничего, а разрушается если дуга (ссылка) есть и существующая ссылка другая 
                        lnodes[current].prev = lnodes[current].prev == -1 ? previous : 
                            (lnodes[current].prev == previous ? previous : -2);
                        lnodes[previous].next = lnodes[previous].next == -1 ? current : 
                            (lnodes[previous].next == current ? current : -2);
                    }

                    previous = current;
                }
            }

            // Находим начала цепочек
            List<PrevNext> startpoints = new List<PrevNext>();
            for (int n = 0; n < nnodes; n++) // n - номер, он же код узла
            {
                // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
                // предыдущих несколько или предыдущий один, но у него несколько следующих.
                // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.

                var node = lnodes[n];
                // Критерии принятия: (ссылка назад меньше нуля или ссылка назад есть но у того узла ссылки вперед нет) и есть ссылка вперед
                if ((node.prev < 0 || lnodes[node.prev].next < 0) && node.next >= 0)
                { startpoints.Add(node); } // принято
                else // не принято
                { }
            }
            Console.WriteLine($"# startpoins: {startpoints.Count}");

            // Отслеживаем цепочки
            List<PrevNext> maxchain = new List<PrevNext>();
            foreach (var spoint in startpoints)
            {
                PrevNext ndd = spoint;
                // добавлен начальный узел, у которого есть следующий
                List<PrevNext> chain = new List<PrevNext>(new PrevNext[] { ndd });
                while (true)
                {
                    PrevNext ndd_candidate = lnodes[ndd.next];
                    // Если кандидат не имеет предыдущего, то цепочка закончилась
                    if (ndd_candidate.prev < 0) break;
                    // Если кандидат не имеет следующего, то включить в цепочку и выйти
                    if (ndd_candidate.next < 0)
                    {
                        chain.Add(ndd_candidate);
                        break;
                    }
                    // Просто включить в цепочку
                    chain.Add(ndd_candidate);
                    ndd = ndd_candidate;
                }
                if (chain.Count > maxchain.Count) maxchain = chain;
            }
            Console.WriteLine($"maxchain: {maxchain.Count}");

            //// Выдача максимальной цепочки
            //Console.Write(UnCombine(ccodes[maxchain[1].prev], nsymbols));
            //for (int i = 0; i < maxchain.Count - 1; i++)
            //{
            //    int code = maxchain[i].next;
            //    var word = ccodes[code];
            //    string sword = UnCombine(word, nsymbols);
            //    Console.Write(sword[sword.Length - 1]);
            //}
            //Console.WriteLine();
            
            sw.Stop();
            Console.WriteLine($"total duration={sw.ElapsedMilliseconds}");

        }

        struct PrevNext 
        {
            // Ссылки означают >= 0 - номер узла, -1 - не проставлялась, -2 - разрушена
            public int prev, next; 
        }
        
        public static ulong Combine(string sword)
        {
            ulong w = 0;
            for (int i = 0; i < sword.Length; i++)
            {
                char c = sword[i];
                ulong bits = 0;
                if (c == 'A') bits = 0;
                else if (c == 'C') bits = 1;
                else if (c == 'G') bits = 2;
                else bits = 3; // (c == 'T') и другие варианты
                w = (w << 2) | bits;
            }
            return w;
        }
        private static char[] symbols = new char[] { 'A', 'C', 'G', 'T' };
        public static string UnCombine(ulong word, int len)
        {
            char[] char_arr = new char[len];
            ulong w = word;
            for (int i = 0; i < len; i++)
            {
                char_arr[len - i - 1] = symbols[w & 3];
                w >>= 2;
            }
            return new string(char_arr);
        }
    }
}
