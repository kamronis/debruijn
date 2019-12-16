using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeBruijnTestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //TextWriter line_twriter = new StreamWriter(File.Open(@"D:\PROJECTS\DeBrein\100mil_line.txt", FileMode.Create, FileAccess.Write)); //файл со строкой
            //TextWriter reads_twriter = new StreamWriter(File.Open(@"D:\PROJECTS\DeBrein\100mil_reads.txt", FileMode.Create, FileAccess.Write)); //файл с ридами
            TextWriter line_twriter = new StreamWriter(File.Open(@"D:\Home\data\DeBruijn\line.txt", FileMode.Create, FileAccess.Write)); //файл со строкой
            TextWriter reads_twriter = new StreamWriter(File.Open(@"D:\Home\data\DeBruijn\Gen_reads.txt", FileMode.Create, FileAccess.Write)); //файл с ридами
            long size = 50_000_000; //размер исходной строки
            int coverage = 10; // сколько раз полностью покрыть строку с помощью 100 символов
            long reads_count = (size / 100) * coverage; //количество ридов в результате
            string chars = "ACGT";
            char[] stringChars = new char[size];
            Random random = new Random(777777777);

            for (long i = 0; i < stringChars.Length; i++) //генерация файла строки
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            line_twriter.WriteLine(stringChars);
            //var finalString = new String(stringChars);

            line_twriter.Close();
            Console.WriteLine("Line generation finished");
            for (int i = 0; i < reads_count; i++) //генерация файла с ридами
            {
                long position = LongRandom(0, size, random);
                if (position + 100 <= size)
                {
                    String read = "";
                    for (int j = 0; j < 100; j++)
                    {
                        read += stringChars[position + j];
                    }
                    reads_twriter.WriteLine(read);
                }

                
            }
            reads_twriter.Close();
            Console.WriteLine("Successfuly generated!");
        }

        static long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

    }
}
