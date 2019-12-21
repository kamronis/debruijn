using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        public static void Main31()
        {
            Console.WriteLine("Start DeBruijnOrtho Main31");

            // Построение потока ввода
            sw.Restart();
            TextReader treader = new StreamReader(File.Open(Options.readsfilename, FileMode.Open, FileAccess.Read));

            // Рабочий поток
            Stream bytestream = File.Open(Options.bytereadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bwriter = new BinaryWriter(bytestream);

            sw.Restart();
            long nreeds = 0;
            // Резервируем место для количества ридов
            bwriter.Write(nreeds);
            string str = "ACGT";

            string line;
            while ((line = treader.ReadLine()) != null)
            {
                if (nreeds >= Options.readslimit) break; 
                if (nreeds % 1_000_000 == 0) Console.Write($"{nreeds / 1_000_000} ");
                nreeds++;
                
                int nwords = line.Length - Options.nsymbols + 1;

                // Переводим линию в массив байтов
                int nline = line.Length;
                byte[] reed = new byte[nline];
                for (int i = 0; i < reed.Length; i++)
                {
                    char c = line[i];
                    int pos = str.IndexOf(c);
                    if (pos == -1) pos = 3;
                    reed[i] = (byte)pos;
                }

                //// В цикле формируем слов, синтаксически кодируем их, записываем бинарно
                //for (int nom = 0; nom < nwords; nom++)
                //{
                //    string word = line.Substring(nom, Options.nsymbols);
                //    var cword = DBNode.Combine(word);
                //}

                // Записываем длину бинарного рида
                bwriter.Write((long)reed.Length);
                // Записываем массив байтов
                bwriter.Write(reed);
            }
            Console.WriteLine();
            // Записываем получившееся количество ридов, сбрасываем буфера
            bytestream.Position = 0L;
            bwriter.Write(nreeds);
            bwriter.Close();

            sw.Stop();
            Console.WriteLine($"Create binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
        }

    }
}