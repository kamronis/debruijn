using System;
using System.Collections.Generic;
using System.IO;

namespace DeBruijnHisto
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Start DeBruijnHisto version 1.0, file: {HistoOptions.readsfilename} passes: {HistoOptions.npasses} K: {HistoOptions.nsymbols}");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Данные читаются из файла, "слово" в n-граммном разбиении имеет длину nsymbols. 
            bool binaryreads = HistoOptions.readsfilename.EndsWith(".bin");

            sw.Start(); // запускаем секундомер

            // Входной файл ридов
            FileStream filereads = File.Open(HistoOptions.readsfilename, FileMode.Open, FileAccess.Read);

            // Возможен текстовый ридер или бинарный ридер
            TextReader reader = binaryreads ? null : new StreamReader(filereads);
            BinaryReader breader = binaryreads ? new BinaryReader(filereads) : null;

            // Для преобразования входного файл в бинарный формат
            string biochars = "ACGT";

            long numberofnodes = 0;
            int[] nodesrange = new int[100];
            
            // Заведу массив битов на каждый рид. true означает, что слово, начинающаася с этого номера - однократно
            System.Collections.BitArray[] single = null;

            // Сканируем данные, вычисляем узлы
            for (int ipass = 0; ipass < HistoOptions.npasses; ipass++)
            {
                Console.WriteLine($"pass {ipass}: ");
                filereads.Position = 0L;

                long iread = 0, nreads = 0;

                // Нам понадобится словарь
                Dictionary<ulong, HistoInfo> hdic = new Dictionary<ulong, HistoInfo>();

                if (binaryreads)
                {
                    nreads = breader.ReadInt64();
                }
                while (true)
                {
                    int readlength;
                    byte[] bread;
                    if (binaryreads)
                    {
                        readlength = breader.ReadInt32();
                        int comp_length = readlength / 4 + (readlength % 4 == 0 ? 0 : 1);
                        byte[] comp_breed = breader.ReadBytes(comp_length);

                        bread = new byte[readlength];
                        for (int i = 0; i < readlength; i++)
                        {
                            bread[i] = (byte)((comp_breed[i >> 2] >> ((i & 3) << 1)) & 3);
                        }
                        //if (iread == 1499999)
                        //{
                        //    Console.WriteLine($"ir={iread} ");
                        //    for (int k = 0; k < bread.Length; k++)
                        //    {
                        //        Console.Write($"{bread[k]} ");
                        //    }
                        //    Console.WriteLine();
                        //}

                    }
                    else
                    {
                        string line = reader.ReadLine();
                        if (line == null) { nreads = iread; break; }
                        iread++;
                        readlength = line.Length;
                        bread = new byte[readlength];
                        int ind = 0;
                        foreach (char c in line)
                        {
                            int pos = biochars.IndexOf(c);
                            if (pos == -1) pos = 3;
                            bread[ind] = (byte)pos;
                            ind++;
                        }
                    }
                    if (readlength < HistoOptions.nsymbols + 2) continue;


                    // создадим кодированный рид
                    //Word[] reed = null;
                    //reed = new Word[readlength - HistoOptions.nsymbols + 1];

                    for (int i = 0; i < readlength - HistoOptions.nsymbols + 1; i++)
                    {
                        ulong bword = 0;
                        for (int j = 0; j < HistoOptions.nsymbols; j++)
                        {
                            // сдвигаем влево и делаем "или" с байтом
                            bword = (bword << 2) | bread[i + j];
                        }

                        //if (iread == 1500000 && i == 0) Console.WriteLine("bword(0)=" + bword);

                        // Работаем только со словами данного прохода
                        if ((int)(bword & (ulong)(HistoOptions.npasses - 1)) == ipass)
                        {
                            HistoInfo hinfo;
                            if (hdic.TryGetValue(bword, out hinfo))
                            {
                                //hinfo.count++;
                                hinfo.count = hinfo.count + 1;
                            }
                            else
                            {
                                hdic.Add(bword, new HistoInfo());
                            }
                        }
                    }
                    iread++;
                    if (iread >= nreads) break;
                }
                // Фиксируем результат
                numberofnodes += hdic.Count;
                foreach (KeyValuePair<ulong, HistoInfo> pair in hdic)
                {
                    int nom = pair.Value.count;
                    if (nom >= nodesrange.Length) nom = nodesrange.Length - 1;
                    nodesrange[nom]++;
                }

                // Снова сканируем этот проход на предмет выявления и фиксации однократных узлов (только для бинарных ридов)
                if (binaryreads)
                {
                    filereads.Position = 0L;
                    nreads = breader.ReadInt64();
                    if (single == null) single = new System.Collections.BitArray[nreads];

                    for (long ir = 0; ir <nreads; ir++)
                    {
                        int readlength = breader.ReadInt32();
                        int comp_length = readlength / 4 + (readlength % 4 == 0 ? 0 : 1);
                        byte[] comp_breed = breader.ReadBytes(comp_length);
                        byte[] bread = new byte[readlength];
                        for (int i = 0; i < readlength; i++)
                        {
                            bread[i] = (byte)((comp_breed[i >> 2] >> ((i & 3) << 1)) & 3);
                        }
                        if (readlength < HistoOptions.nsymbols + 2) continue;
                        // создадим кодированный рид
                        //Word[] reed = null;
                        //reed = new Word[readlength - HistoOptions.nsymbols + 1];

                        //if (ir == 1499999)
                        //{
                        //    Console.WriteLine($"iread={ir}");
                        //    for (int k = 0; k < bread.Length; k++)
                        //    {
                        //        Console.Write($"{bread[k]} ");
                        //    }
                        //    Console.WriteLine();
                        //}


                        if (single[ir] == null) 
                            single[ir] = new System.Collections.BitArray(readlength - HistoOptions.nsymbols + 1);

                        for (int i = 0; i < readlength - HistoOptions.nsymbols + 1; i++)
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
                                    // Выявляем однократность узла. Другие кратности так не подсчитать
                                    if (hinfo.count == 0)
                                    {
                                        //Console.WriteLine($"=={++cnt} {bword}");
                                        single[ir][i] = true;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Err in: ir={ir} i={i}");
                                    for (int k = 0; k < bread.Length; k++)
                                    {
                                        Console.Write($"{bread[k]} ");
                                    }
                                    Console.WriteLine();
                                    Console.WriteLine(bword);
                                    throw new Exception("Err: Already must be coded word");
                                }
                            }
                        }
                    }
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

            for (int ii = 0; ii < single.Length; ii++)
            {
                int nbits = 0;
                bool was = false;
                for (int jj = 0; jj < single[ii].Length; jj++)
                {
                    if (single[ii][jj])
                    { // единственный
                        if (jj == 0) Console.Write("[");
                        nbits += 1;
                        was = true;
                    }
                    else
                    { // множественный
                        if (nbits > 0)
                        {
                            Console.Write($"{nbits} ");
                        }
                        nbits = 0;
                    }
                }
                if (was)
                {
                    if (nbits > 0) Console.Write($"{nbits}]");
                    Console.WriteLine();
                }
            }
        }
    }
}
