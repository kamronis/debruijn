using System;
using System.Collections.Specialized;
using System.IO;

namespace ReadsGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileout;
            long chainlength;
            int readlength, multiplicity;
            if (args.Length != 4)
            {
                Console.WriteLine("usage: ReadsGen fileout chainlength readlength multiplicity");
                fileout = @"D:\Home\data\deBruijn\G_reads.bin";
                chainlength = 100_000_000;
                readlength = 100;
                multiplicity = 10;
            }
            else
            {
                fileout = args[0];
                chainlength = Int64.Parse(args[1]);
                readlength = Int32.Parse(args[2]);
                multiplicity = Int32.Parse(args[3]);
            }
            Console.Write($"Start ReadsGen: {fileout} {chainlength} {readlength} {multiplicity}");

            // Формирование цепочки символов, символы 0-3 упаковываются в байты. Это позволяет иметь цепочку до 8 млрд. символов
            int byteslength = (int)((chainlength / 4) + 1);
            byte[] bytes = new byte[byteslength];
            Random rnd = new Random();
            // Двойной цикл формирования массива
            for (int i = 0; i < byteslength; i++)
            {
                int bt = 0;
                for (int j = 0; j < 4; j++)
                {
                    bt = bt << 2;
                    int symb = rnd.Next(4);
                    bt = bt | symb;
                }
                bytes[i] = (byte)bt;
            }

            // Бинарный райтер для записи ридов
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(fileout));
            long nreads = chainlength / (long)readlength * (long)multiplicity;
            bw.Write(nreads);

            for (long ii = 0; ii < nreads; ii++)
            {
                bw.Write(readlength);

                int comp_length = readlength / 4 + (readlength % 4 == 0 ? 0 : 1);
                byte[] comp_breed = new byte[comp_length];

                // начальный номер байта в массиве
                int bytestartposition = rnd.Next(bytes.Length - comp_length);
                // номер символа в массиве
                long chainposition = (((long)bytestartposition) << 2) | (long)rnd.Next(4);
                //int bt = 0, inbt = 0;
                for (int i = 0; i < readlength; i++)
                {
                    int symb = ((int)bytes[chainposition >> 2] >> ((int)(chainposition & 3) << 1)) & 3;
                    chainposition++;
                    comp_breed[i >> 2] = (byte)(comp_breed[i >> 2] | (symb << ((i & 3) << 1)));
                }
                // Записываем массив байтов
                bw.Write(comp_breed);
            }
            bw.Close();
        }
    }
}
