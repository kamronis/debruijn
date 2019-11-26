using System;
using System.IO;

namespace Experiments
{
    class Program
    {
        /// <summary>
        /// Читаю бинарные файлы (списков узлов) формата nlist и пишу один выходной того же формата
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
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
