using System;
using System.IO;
using System.Linq;

namespace ReadsProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Reads Processing!");
            FileStream filereads = File.Open(@"D:\PROJECTS\DeBrein\500k_1_reads.txt", FileMode.Open, FileAccess.Read);
            TextReader reader = new StreamReader(filereads);

            // Сделаем байт-нарный файл ридов, его структура [[byte]] и бинарные ридер и райтер к нему
            FileStream filebytereads = File.Open(@"D:\PROJECTS\DeBrein\bytereads_compressed.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bwriter = new BinaryWriter(filebytereads);

            // Преобразуем входной файл в бинарный
            bwriter.Write(0L); // резервируем
            string biochars = "ACGT";
            long nreads = 0;
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
 
                int comp_length = nline / 4 + (nline % 4 == 0 ? 0 : 1);
                byte[] comp_breed = new byte[comp_length];
                for (int i = 0; i < breed.Length; i++)
                {
                    byte tmp = (byte)(breed[i] << ((2*i)%8));
                    comp_breed[i / 4] = (byte)(comp_breed[i / 4] | tmp);

                }
                // Записываем длину бинарного рида
                bwriter.Write((int)breed.Length);
                // Записываем массив байтов
                bwriter.Write(comp_breed);
            }
            filebytereads.Position = 0L;
            //bwriter.Seek(0, SeekOrigin.Begin);
            bwriter.Write((long)nreads);
            bwriter.Close();
            filebytereads.Close();
        }
    }
}
