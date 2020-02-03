using System;
using System.Collections.Generic;
using System.IO;

namespace DeBruijnHisto
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine($"Start DeBruijnHisto version 0.9, passes: {HistoOptions.npasses} K: {HistoOptions.nsymbols}");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 

            sw.Start(); // запускаем секундомер

            // Входной файл ридов
            FileStream filereads = File.Open(HistoOptions.readsfilename, FileMode.Open, FileAccess.Read);
            TextReader reader = new StreamReader(filereads);

            // Для преобразования входного файл в бинарный формат
            string biochars = "ACGT";

            long nreads = 0;
            long numberofnodes = 0;
            int[] nodesrange = new int[24];
            // Сканируем данные, вычисляем узлы
            for (int ipass = 0; ipass < HistoOptions.npasses; ipass++)
            {
                Console.WriteLine($"pass {ipass}: ");
                filereads.Position = 0L;

                // Нам понадобится словарь
                Dictionary<ulong, HistoInfo> hdic = new Dictionary<ulong, HistoInfo>();

                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    int len = line.Length;
                    byte[] bread = new byte[len];
                    int ind = 0;
                    foreach (char c in line)
                    {
                        int pos = biochars.IndexOf(c);
                        if (pos == -1) pos = 3;
                        bread[ind] = (byte)pos;
                        ind++;
                    }

                    // создадим кодированный рид
                    Word[] reed = null;
                    reed = new Word[len - HistoOptions.nsymbols + 1];

                    for (int i = 0; i < len - HistoOptions.nsymbols + 1; i++)
                    {
                        ulong bword = 0;
                        for (int j = 0; j < HistoOptions.nsymbols; j++)
                        {
                            // сдвигаем влево и делаем "или" с байтом
                            bword = (bword << 2) | bread[i + j];
                        }

                        // Работаем только со словами данного прохода
                        if ((int)(bword & (ulong)(HistoOptions.npasses - 1)) == ipass)
                        {
                            HistoInfo hinfo;
                            if (hdic.TryGetValue(bword, out hinfo))
                            {
                                hinfo.count++;
                            }
                            else
                            {
                                hdic.Add(bword, new HistoInfo());
                            }
                        }
                    }
                }
                // Фиксируем результат
                numberofnodes += hdic.Count;
                foreach (KeyValuePair<ulong, HistoInfo> pair in hdic)
                {
                    int nom = pair.Value.count;
                    if (nom >= nodesrange.Length) nom = nodesrange.Length - 1;
                    nodesrange[nom]++;
                }
                // Освобождаю память
                hdic = new Dictionary<ulong, HistoInfo>();
                GC.Collect();
            }
            // Печатаем результат
            for (int i=0; i< nodesrange.Length; i++)
            {
                Console.WriteLine($"{i+1} \t {nodesrange[i]}");
            }

            Console.WriteLine($"###nodes total: {numberofnodes}");
            sw.Stop();
            Console.WriteLine($"duration={sw.ElapsedMilliseconds}");
        }
    }
}
