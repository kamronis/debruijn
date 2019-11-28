using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        private static int nsymbols = 20;
        public static void Main3()
        {
            Console.WriteLine("Start DeBruijnNametable Main3");



            // Построение потока ввода
            sw.Restart();
            TextReader treader = new StreamReader(File.Open(Options.readsfilename, FileMode.Open, FileAccess.Read));
            Stream tmpbinstream = File.Open(Options.breadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(tmpbinstream);

            sw.Restart();
            long nreeds = 0;
            // Резервируем место для количества ридов
            bw.Write(nreeds);
            string line;
            while ((line = treader.ReadLine()) != null)
            {
                //if (nreeds > 50000) break; /////////////////  ОТЛАДКА!
                if (nreeds % 100_000 == 0) Console.Write($"{nreeds / 100_000} ");
                nreeds++;
                // Записываем длину бинарного рида
                int nwords = line.Length - nsymbols + 1;
                bw.Write((long)nwords);
                // В цикле формируем слов, синтаксически кодируем их, записываем бинарно
                for (int nom = 0; nom < nwords; nom++)
                {
                    string word = line.Substring(nom, nsymbols);
                    var cword = DBNode.Combine(word);
                    bw.Write(cword);
                }
            }
            Console.WriteLine();
            // Записываем получившееся количество ридов, сбрасываем буфера
            tmpbinstream.Position = 0L;
            bw.Write(nreeds);
            bw.Flush();

            sw.Stop();
            Console.WriteLine($"Create binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
            tmpbinstream.Close();
        }

    }
}