using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DeBruijnDirect
{
    partial class Program
    {
        public static void Main2()
        {
            Console.WriteLine($"Start DeBruijnDirect (Main2) version 1.2, passes: {DirectOptions.npasses}, sections: {DirectOptions.nsections} K: {DirectOptions.nsymbols}");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 

            sw.Start(); // запускаем секундомер

            // Входной файл ридов
            FileStream filecompbytereads = File.Open(DirectOptions.compressedreadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader bcompreader = new BinaryReader(filecompbytereads);

            // Определяем ключевые функции
            ulong pathmask = (ulong)(DirectOptions.npasses - 1);
            int pm = DirectOptions.npasses - 1;
            int pathshift = 0;
            while (pm != 0) { pathshift++; pm = pm >> 1; }
            Func<ulong, int> word2path = w => (int)(w & pathmask); // В этой программе - не нужна!
            ulong secmask = (ulong)(DirectOptions.nsections - 1);
            Func<ulong, int> word2sec = w => (int)((w >> pathshift) & secmask);


            // Главный параметр: число секций DirectOptions.nsections. Количество проходов = 1

            // Словарь, разбитый по секциям. Отнесение слов к секции - по младшим битам слова
            Dictionary<ulong, Code>[] dics = Enumerable.Range(0, DirectOptions.nsections)
                .Select(i => new Dictionary<ulong, Code>()).ToArray();

            // Накопленное количество узлов, разбитое по секциям
            int[] nnods = Enumerable.Repeat<int>(0, DirectOptions.nsections).ToArray();
            // Части формируемого графа
            List<ulong>[] wlists = Enumerable.Repeat<int>(0, DirectOptions.nsections)
                .Select(i => new List<ulong>())
                .ToArray();
            List<PrevNext>[] llists = Enumerable.Repeat<int>(0, DirectOptions.nsections)
                .Select(i => new List<PrevNext>())
                .ToArray();

            // Сканируем новые кодированные риды
            BinaryReader br = bcompreader;
            long nreads = br.ReadInt64();
            long nwords = 0L;
            
            Console.WriteLine("Constructing graph: ");
            for (int nom = 0; nom < nreads; nom++)
            {
                if (nom % 1000000 == 0) { Console.Write($"{nom / 1000000} "); }

                int len1 = (int)br.ReadInt32();
                int comp_len = len1 / 4 + (len1 % 4 == 0 ? 0 : 1);

                byte[] bcompread = br.ReadBytes(comp_len);
                byte[] bread1 = new byte[len1];
                for (int i = 0; i < bread1.Length; i++)
                {
                    bread1[i] = (byte)((bcompread[i / 4] >> ((2 * i) % 8)) & 3);
                }

                Code previous = new Code(-1);
                for (int i = 0; i < len1 - DirectOptions.nsymbols + 1; i++)
                {
                    UInt64 wd = 0;
                    for (int j = 0; j < DirectOptions.nsymbols; j++)
                    {
                        // сдвигаем влево и делаем "или" с байтом
                        wd = (wd << 2) | bread1[i + j];
                    }
                    ulong bword = wd;
                    int inpath = word2path(bword);
                    int insec = word2sec(bword);
                    nwords++;

                    // находим или создаем текущий узел
                    Code code;
                    if (dics[insec].TryGetValue(bword, out code)) 
                    { 
                    }
                    else
                    {
                        code = new Code(insec, nnods[insec]);
                        dics[insec].Add(bword, code);
                        nnods[insec] += 1;
                        // Добавляем в списки
                        //wlists[insec].Add(bword);
                        llists[insec].Add(new PrevNext() { prev = new Code(-1), next = new Code(-1) });
                    }
                    Code current = code;
                    if (!previous.Undefined)
                    {
                        //// дуга добавляется если нет ничего, а разрушается если дуга (ссылка) есть и существующая ссылка другая 
                        //lnodes[current.Sec][current.Nom].prev = lnodes[current.Sec][current.Nom].prev.Undefined ? previous :
                        //    (lnodes[current.Sec][current.Nom].prev.Value == previous.Value ? previous : new Code(-2));
                        //lnodes[previous.Sec][previous.Nom].next = lnodes[previous.Sec][previous.Nom].next.Undefined ? current :
                        //    (lnodes[previous.Sec][previous.Nom].next.Value == current.Value ? current : new Code(-2));
                        llists[current.Sec][current.Nom] = new PrevNext()
                        {
                            prev = llists[current.Sec][current.Nom].prev.Undefined ? previous :
                            (llists[current.Sec][current.Nom].prev.Value == previous.Value ? previous : new Code(-2)),
                            next = llists[current.Sec][current.Nom].next
                        };
                        llists[previous.Sec][previous.Nom] = new PrevNext()
                        {
                            prev = llists[previous.Sec][previous.Nom].prev,
                            next = llists[previous.Sec][previous.Nom].next.Undefined ? current :
                            (llists[previous.Sec][previous.Nom].next.Value == current.Value ? current : new Code(-2))
                        };
                    }

                    previous = current;
                }
            }
            // Подсчет числа кодов
            long ncodes = 0L;
            foreach (long nn in nnods) ncodes += nn;

            Console.WriteLine();
            Console.WriteLine($"reads: {nreads} words: {nwords} codes: {ncodes}");

            // Находим начала цепочек
            List<PrevNext> startpoints = new List<PrevNext>();
            // Двойной цикл по узлам
            for (int isec = 0; isec < nnods.Length; isec++)
            {
                for (int nom = 0; nom < nnods[isec]; nom++) // n - номер, он же код узла
                {
                    // Основная идея в том, что цепочка может начинаться ТОЛЬКО с узла у которого нет предыдущего или 
                    // предыдущих несколько или предыдущий один, но у него несколько следующих.
                    // Кроме того, нет смысла рассматривать те узлы, у которых не единственный следующий.

                    // Есть секция isec и номер узла в секции nom. Читаем узел
                    var node = llists[isec][nom];
                    // Критерии принятия: (ссылка назад меньше нуля или ссылка назад есть но у того узла ссылки вперед нет) и есть ссылка вперед
                    if ((node.prev.Value < 0 || llists[node.prev.Sec][node.prev.Nom].next.Value < 0) && node.next.Value >= 0)
                    { startpoints.Add(node); } // принято
                    else // не принято
                    { }
                }
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
                    PrevNext ndd_candidate = llists[ndd.next.Sec][ndd.next.Nom];
                    // Если кандидат не имеет предыдущего, то цепочка закончилась
                    if (ndd_candidate.prev.Value < 0) break;
                    // Если кандидат не имеет следующего, то включить в цепочку и выйти
                    if (ndd_candidate.next.Value < 0)
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
            Console.WriteLine($"Duration={sw.ElapsedMilliseconds}");
        }
    }
}
