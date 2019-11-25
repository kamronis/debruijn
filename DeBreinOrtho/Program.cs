using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBreinOrtho
{
    partial class Program
    {
        /// <summary>
        /// Программа запускается в режиме мастера или слэйва (хранилище) 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Start DeBreinOrtho!");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            // размер порции
            int nsymbols = 20;

            bool ismaster = true;
            //if (args.Length > 1) { ismaster = false; } // Какой-то вариант срежима слэйва

            // Есть хранилище. Оно формируется и для мастера и для слэйва
            StorageClient storage = new StorageClientMaster(nsymbols, ismaster);

            // Активность проявляет только мастер, он реализует алгоритм
            if (!ismaster)
            {
                return;
            }

            // ================================== Master =============================
            sw.Restart();
            // Файл с ридами
            string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            // Создаем буферизированную обработку ридов
            Polar.DB.BufferredProcessing<Reed> orthoexec = new Polar.DB.BufferredProcessing<Reed>(1000, reeds =>
            {
                Reed.ProcessLines(reeds, storage);
            });
            // Сканируем данные, вычисляем узлы
            using (TextReader reader = new StreamReader(File.Open(readsfilename, FileMode.Open, FileAccess.Read)))
            {
                int lcount = 0;
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    lcount++;
                    //if (lcount >= 10000) break;
                    Reed reed = new Reed(line, nsymbols, storage);
                    orthoexec.Add(reed);

                }
                orthoexec.Flush();

                Console.WriteLine($"lines:{lcount} nodes: {storage.NodesCount()}");
            }
            sw.Stop();
            Console.WriteLine("Graph ok. Duration=" + sw.ElapsedMilliseconds);
            Console.WriteLine($"TotalMemory used: {GC.GetTotalMemory(true)}");

            sw.Restart();
            ((StorageClientMaster)storage).Statistics();
            sw.Stop(); Console.WriteLine($"Build statistics ok. Duration={sw.ElapsedMilliseconds}");

            sw.Restart();
            ((StorageClientMaster)storage).ExtractChains();
            sw.Stop(); Console.WriteLine($"Extract chains ok. Duration={sw.ElapsedMilliseconds}");


        }

    }
}
