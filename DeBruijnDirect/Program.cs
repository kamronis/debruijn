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
            string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            int nsymbols = 20;

            sw.Start(); // запускаем секундомер

            // Две важнейших структуры и одна временная
            List<ulong> ccodes = new List<ulong>();
            List<int[]> creads = new List<int[]>();
            Dictionary<ulong, int> dic = new Dictionary<ulong, int>();
            int nnodes = 0;

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
                    
                    // создадим или прочитаем кодированный рид
                    int[] reed = new int[len - nsymbols + 1];
                    creads.Add(reed);
                    
                    for (int i = 0; i < len - nsymbols + 1; i++)
                    {
                        // формируем слово
                        string word = line.Substring(i, nsymbols);
                        nwords++;
                        // Переводим слово в бинарный вид
                        ulong bword = Combine(word);
                        // находим или создаем текущий узел
                        int code = ccodes.Count;
                        if (dic.TryGetValue(bword, out code)) { }
                        else
                        {
                            ccodes.Add(bword);
                            nnodes++;
                            dic.Add(bword, code);
                        }
                        reed[i] = code;
                    }

                }
                Console.WriteLine($"memory used after dictionaries: {GC.GetTotalMemory(false)}");
                Console.WriteLine($"lines:{lcount} words: {nwords} codes: {ccodes.Count}");
                // Теперь нам словарь не поднадобится
                dic = new Dictionary<ulong, int>();
                GC.Collect();
            }

            sw.Stop();
            Console.WriteLine($"duration={sw.ElapsedMilliseconds}");

            // Теперь нам нужны узлы со ссылками (номерами) prev и next
            PrevNext[] lnodes = new PrevNext[nnodes];

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
            Console.WriteLine($"memory used after graph building: {GC.GetTotalMemory(false)}");

            // Находим начала цепочек
            List<PrevNext> startpoints = new List<PrevNext>();
            for (int n = 0; n < nnodes; n++) // n - номер, он же код узла
            {
                // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
                // предыдущих несколько или предыдущий один, но у него несколько следующих.
                // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.
                
                // Критерии непринятия: ссылка назад  должна быть меньше нуля
            }

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
