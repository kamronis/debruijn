using System;
using System.IO;

namespace Experiments
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Start Experiments Main");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            string path = @"D:\Home\data\deBruijn\";
            int nsymbols = 20;

            sw.Restart();

            // Рабочий поток
            Stream binstream = File.Open(path + "outbinstream.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Stream bytestream = binstream; // new BufferedStream(binstream); //
            BinaryWriter bwriter = new BinaryWriter(bytestream);

            bool toload = false;

            if (toload)
            {
                sw.Restart();
                TextReader treader = new StreamReader(File.Open(path + "Gen_reads.txt", FileMode.Open, FileAccess.Read));
                long nreeds = 0;
                // Резервируем место для количества ридов
                bwriter.Write(nreeds);
                char[] chrs = new char[] { 'A', 'C', 'G', 'T' };
                string str = "ACGT";

                string line;
                while ((line = treader.ReadLine()) != null)
                {
                    if (nreeds % 1_000_000 == 0) Console.Write($"{nreeds / 1_000_000} ");
                    nreeds++;
                    int nline = line.Length;
                    byte[] reed = new byte[nline];
                    for (int i = 0; i < reed.Length; i++)
                    {
                        char c = line[i];
                        int pos = str.IndexOf(c);
                        if (pos == -1) pos = 3;
                        reed[i] = (byte)pos;
                    }

                    // Записываем длину бинарного рида
                    bwriter.Write((long)reed.Length);
                    // Записываем массив байтов
                    bwriter.Write(reed);
                }
                Console.WriteLine();
                // Записываем получившееся количество ридов, сбрасываем буфера
                bytestream.Position = 0L;
                bwriter.Write(nreeds);
                bwriter.Flush();

                sw.Stop();
                Console.WriteLine($"Create binary reeds file ok. nreeds: {nreeds} duration: {sw.ElapsedMilliseconds}");
                bytestream.Flush();
            }
            else
            { // Работа с рабочим файлом
                sw.Restart();
                BinaryReader breader = new BinaryReader(bytestream);
                bytestream.Position = 0L;
                long nreeds = breader.ReadInt64();
                for (long ii=0; ii<nreeds; ii++)
                {
                    int len = (int)breader.ReadInt64();
                    byte[] arr = breader.ReadBytes(len);
                    // Формируем поток слов
                    int nwords = len - nsymbols + 1;
                    for (int i=0; i<nwords; i++)
                    {
                        UInt64 word = 0;
                        for (int j=0; j<nsymbols; j++)
                        {
                            // сдвигаем влево и делаем "или" с байтом
                            word = (word << 2) | arr[i + j];
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine($"binary reeds file ok. nreeds: {nreeds} duration: {sw.ElapsedMilliseconds}");
            }
        }


        /// <summary>
        /// Читаю бинарные файлы (списков узлов) формата nlist и пишу один выходной того же формата
        /// </summary>
        /// <param name="args"></param>
        static void Main1()
        {
            Console.WriteLine("Merging nlist files");
            string[] filenames = new string[] { @"D:\Home\data\deBrein\nlist.bin", @"D:\Home\data\deBrein\nlist_net1.bin" };
            string outfilename = @"D:\Home\data\deBrein\nlist$$$.bin";
            int nparts = filenames.Length;
            
            int nshift = 0; // Число битов сдвига чтобы из кода получить номер в подпоследовательности
            int mask = nparts - 1;
            while (mask != 0) { nshift++; mask = mask >> 1; }
            mask = nparts - 1; // маска для выделения номера секции 

            // Части будут располагаться по очереди и "в стык", сначала нулевая, потом первая. 
            long nelements = 0L; // Всего (будет) записано
            long[] begs = new long[nparts + 1];
            begs[0] = 0L;
            BinaryReader reader;
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(outfilename));

            Func<int, int> CorrectLink = (int linkin) =>
            {
                if (linkin < 0) return linkin;
                int p = linkin & mask;
                int nom = linkin >> nshift;
                int linkout = (int)begs[p] + nom;
                return linkout;
            };

            // Пока пишем -1
            writer.Write(-1L);
            for (int ipart=0; ipart<nparts; ipart++)
            {
                reader = new BinaryReader(File.OpenRead(filenames[ipart]));
                // Длина списка
                long len = reader.ReadInt64();
                nelements += len;
                begs[ipart + 1] = nelements;
                for (long i=0; i<len; i++)
                {
                    //public struct CNode
                    //{
                    //    public UInt64 bword;
                    //    public int prev;
                    //    public int next;
                    //}
                    UInt64 bword = reader.ReadUInt64();
                    int prev = reader.ReadInt32();
                    int next = reader.ReadInt32();
                    writer.Write(bword);
                    writer.Write(CorrectLink(prev));
                    writer.Write(CorrectLink(next));
                }
                reader.Close();
            }
            // Парепишем количество элементов
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(nelements);
            writer.Close();
        }
    }
}
