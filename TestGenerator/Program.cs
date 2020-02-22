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
            //TextWriter line_twriter = new StreamWriter(File.Open(@"C:\data\DeBruijn\line.txt", FileMode.Create, FileAccess.Write)); //файл со строкой
            //TextWriter reads_twriter = new StreamWriter(File.Open(@"C:\data\DeBruijn\Gen_reads.txt", FileMode.Create, FileAccess.Write)); //файл с ридами
            TextWriter line_twriter = new StreamWriter(File.Open(@"D:\Home\data\DeBruijn\line.txt", FileMode.Create, FileAccess.Write)); //файл со строкой
            TextWriter reads_twriter = new StreamWriter(File.Open(@"D:\Home\data\DeBruijn\Gen_reads.txt", FileMode.Create, FileAccess.Write)); //файл с ридами
            
            long size = 10_000_000; //размер исходной строки
            int readLength = 100;
            int coverage = 10; // сколько раз полностью покрыть строку с помощью 100 символов
            
            long reads_count = (size / readLength) * coverage; //количество ридов в результате
            string chars = "ACGT";
            long innerArrSize = 1000_000_000;
            long outerArrSize = Convert.ToInt64(Math.Ceiling((double)size / innerArrSize));
            char[][] stringChars = new char[outerArrSize][];
            Random random = new Random(777777777);
            Random rnd = new Random();
            for (int j = 0; j < outerArrSize; j++) //генерация файла aстроки
            {
                char[] innerArr = new char[innerArrSize];
                for (long i = 0; i < innerArrSize; i++)
                {
                    innerArr[i] = chars[random.Next(chars.Length)];
                }
                stringChars[j] = innerArr;
                line_twriter.WriteLine(innerArr);
            }
            //line_twriter.WriteLine(stringChars);
            //var finalString = new String(stringChars);

            line_twriter.Close();
            Console.WriteLine("Line generation finished");
            long real_length = 0;
            for (int i = 0; i < reads_count; i++) //генерация файла с ридами
            {
                long position = LongRandom(0, size - readLength + 1, random);
                long position_x = position / innerArrSize;
                long position_y = position % innerArrSize;
                String read = "";
                real_length++;
                //Console.WriteLine(real_length);
                long new_position_y = 0;
                for (int j = 0; j < readLength; j++)
                {
                    if (position_y + j < innerArrSize)
                    {
                        char nxtchr = stringChars[position_x][position_y + j];
                        // Имитация ошибки
                        //if (rnd.Next(1000) < 1) nxtchr = chars[random.Next(chars.Length)];
                        read += nxtchr;
                    }
                    else
                    {
                        char nxtchr = stringChars[position_x + 1][new_position_y];
                        new_position_y++;
                        read += nxtchr;
                    }
                }
                reads_twriter.WriteLine(read);


            }
            reads_twriter.Close();
            Console.WriteLine($"Successfuly generated! Reads generated: {real_length}");
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
