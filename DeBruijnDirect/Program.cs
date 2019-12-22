using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeBruijnDirect
{
    partial class Program
    {
        static void Main()
        {
            Main1();
        }
        static void Main1()
        {
            Console.WriteLine($"Start DeBruijnDirect version 1.1, passes: {DirectOptions.npasses}, K: {DirectOptions.nsymbols}");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 

            sw.Start(); // запускаем секундомер

            // Входной файл ридов
            FileStream filereads = File.Open(DirectOptions.readsfilename, FileMode.Open, FileAccess.Read);
            TextReader reader = new StreamReader(filereads);

            // Сделаем байт-нарный файл ридов, его структура [[byte]] и бинарные ридер и райтер к нему
            FileStream filebytereads = File.Open(DirectOptions.bytereadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bwriter = new BinaryWriter(filebytereads);
            BinaryReader breader = new BinaryReader(filebytereads);

            // Преобразуем входной файл в бинарный
            bwriter.Write(0L); // резервируем
            string biochars = "ACGT";
            int nreads = 0;
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                nreads++;
                // Переводим линию в массив байтов
                int nline = line.Length;
                byte[] breed = new byte[nline];
                for (int i = 0; i < breed.Length; i++)
                {
                    char c = line[i];
                    int pos = biochars.IndexOf(c);
                    if (pos == -1) pos = 3;
                    breed[i] = (byte)pos;
                }
                // Записываем длину бинарного рида
                bwriter.Write((long)breed.Length);
                // Записываем массив байтов
                bwriter.Write(breed);
            }
            bwriter.Seek(0, SeekOrigin.Begin);
            bwriter.Write((long)nreads);
            bwriter.Flush();

            Console.WriteLine("bytereads ok.");
            reader.Close();

            // Две важнейших структуры и одна временная
            List<ulong> ccodes = new List<ulong>(); // не используется!!!

            Dictionary<ulong, int> dic = new Dictionary<ulong, int>();
            int nnodes = 0;

            // Файлы для накапливания кодированных ридов creads.bin
            string f1name = DirectOptions.workdir + "f1.bin";
            FileStream f1 = File.Open(f1name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            string f2name = DirectOptions.workdir + "f2.bin";
            FileStream f2 = File.Open(f2name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Stream[] streams = new Stream[] { f1, f2 };
            int first = 0;
            BinaryReader br = new BinaryReader(streams[first % 2]);
            BinaryWriter bw = new BinaryWriter(streams[(first + 1) % 2]);

            Action TogleFiles = () => 
            {
                f1.Position = 0L;
                f2.Position = 0L;
                first = (first + 1) % 2;
                br = new BinaryReader(streams[first % 2]); br.BaseStream.Flush();
                bw = new BinaryWriter(streams[(first + 1) % 2]); bw.BaseStream.Flush();
            };

            // Сканируем данные, вычисляем узлы
            for (int ipass = 0; ipass < DirectOptions.npasses; ipass++)
            {
                Console.WriteLine($"pass {ipass}: ");
                int nwords = 0;
                filebytereads.Position = 0L;
                long nnreads = breader.ReadInt64();
                if (nnreads != nreads) throw new Exception("2234466");

                // читаем и пишем число ридов в файлы
                if (ipass != 0) { var nn = br.ReadInt64(); if (nn != nreads) throw new Exception("we46546"); }
                bw.Write((long)nreads);
                for (int iline = 0; iline < nreads; iline++)
                {
                    if (iline % 1000_000 == 0) { /*Console.CursorLeft = 0;*/ Console.Write($"{(long)iline * 100L / (long)nreads}% "); }
                    int len = (int)breader.ReadInt64();
                    byte[] bread = breader.ReadBytes(len);

                    // создадим или прочитаем кодированный рид
                    int[] reed = null;
                    if (ipass == 0)
                    {
                        reed = new int[len - DirectOptions.nsymbols + 1];
                    }
                    else
                    {
                        int l_read = (int)br.ReadInt64();
                        reed = new int[l_read];
                        for (int i = 0; i < reed.Length; i++)
                        {
                            int c = br.ReadInt32();
                            reed[i] = c;
                        }
                    }

                    for (int i = 0; i < len - DirectOptions.nsymbols + 1; i++)
                    {
                        UInt64 wd = 0;
                        for (int j = 0; j < DirectOptions.nsymbols; j++)
                        {
                            // сдвигаем влево и делаем "или" с байтом
                            wd = (wd << 2) | bread[i + j];
                        }
                        ulong bword = wd;

                        // находим или создаем текущий узел
                        if ((int)(bword & (ulong)(DirectOptions.npasses - 1)) == ipass)
                        {
                            int code;
                            if (dic.TryGetValue(bword, out code)) { }
                            else
                            {
                                code = nnodes;
                                dic.Add(bword, code);
                                nnodes++;
                            }
                            reed[i] = code;
                        }
                    }
                    // Запишем кодированный рид
                    bw.Write((long)reed.Length);
                    for (int i = 0; i < reed.Length; i++) bw.Write(reed[i]);
                }
                if (ipass == DirectOptions.npasses - 1)
                {
                    Console.WriteLine($"memory used after dictionaries: {GC.GetTotalMemory(false)}");
                    Console.WriteLine($"lines:{nreads} words: {nwords} codes: {nnodes} dictionary : {dic.Count} elements");
                }
                // Меняем файлы местами
                TogleFiles();
                // Теперь нам словарь не поднадобится
                dic = new Dictionary<ulong, int>();
                GC.Collect();
                Console.WriteLine();
            }

            // Теперь нам нужны узлы со ссылками (номерами) prev и next
            PrevNext[] lnodes = new PrevNext[nnodes];
            for (int i=0; i<nnodes; i++) { lnodes[i].prev = -1; lnodes[i].next = -1; }

            // Сканируем новые кодированные риды
            long nr = br.ReadInt64();
            if (nr != nreads) throw new Exception("8754332");
            Console.WriteLine("Constructing graph: ");
            for (int nom = 0; nom < nreads; nom++)
            {
                if (nom % 1000000 == 0) Console.Write($"{nom / 1000000} ");
                long nc = br.ReadInt64();
                int[] cread = new int[nc];
                for (int j = 0; j < nc; j++)
                {
                    int c = br.ReadInt32();
                    cread[j] = c;
                }
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
            Console.WriteLine();

            // Выдача lnodes.bin для проверки
            using (BinaryWriter w = new BinaryWriter(File.Open(DirectOptions.workdir + "lnodes_d.bin", FileMode.Create, FileAccess.Write)))
            {
                w.Write((long)lnodes.Length);
                for (int i = 0; i < lnodes.Length; i++)
                {
                    w.Write(lnodes[i].prev);
                    w.Write(lnodes[i].next);
                }
            }

            Console.WriteLine("Building chains");
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

    }
}
