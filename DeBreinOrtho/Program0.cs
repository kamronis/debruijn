using System;
using System.Collections.Generic;
using System.IO;

namespace DeBreinOrtho
{
    partial class Program0
    {
        static void Main0(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Start DeBreinOrtho!");
            string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            int nsymbols = 20;

            int nwords = 0;

            // Создаем хранилище
            StorageClient storage = new StorageClientMaster(nsymbols, true);
            // Создаем буферизированную обработку
            Polar.DB.BufferredProcessing<Reed> orthoexec = new Polar.DB.BufferredProcessing<Reed>(1000, reeds =>
            {
                Reed.ProcessLines(reeds, storage);
            });

            sw.Start(); // запускаем секундомер
            int variant = 1; // 0 - непосредственный, 1 - буферизированный
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

                    if (variant == 0)
                    {
                        // Непосредственный вариант
                        while (!reed.Finished()) reed.Steps();
                    }
                    if (variant == 1)
                    {
                        // Буферизированный вариант
                        orthoexec.Add(reed);
                    }
                }
                if (variant == 1)
                {
                    // Остаток буферизированного варианта
                    orthoexec.Flush();
                }

                Console.WriteLine($"lines:{lcount} words: {nwords} nodes: {storage.NodesCount()}");
            }
            sw.Stop(); Console.WriteLine($"Build nodes ok. Duration={sw.ElapsedMilliseconds}");
            Console.WriteLine($"$$$$ totalbytessent: {Reed.totalbytessent} totalbytesreceived: {Reed.totalbytesreceived}");
            Console.WriteLine($"TotalMemory: {GC.GetTotalMemory(true)}");

            sw.Restart();
            // Теперь в хранилище есть узлы. По построению они от 0 до NodesCount-1. 
            // Берем последовательно узлы и собираем статистику
            int nisolated = 0, nleftonly = 0, nrightonly = 0, nfull = 0;
            int nfollow = 0, nnotfollow = 0; 
            for (int nd = 0; nd < storage.NodesCount(); nd++)
            {
                DBNode node = storage.GetNode(nd);
                int prev = node.prev;
                int next = node.next;
                if (prev < 0 && next < 0) nisolated++;
                else if (prev >= 0 && next < 0) nleftonly++;
                else if (prev < 0 && next >= 0) nrightonly++;
                else if (prev >= 0 && next >= 0) nfull++;

                if (next >= 0)
                {
                    if (next == nd + 1) nfollow++;
                    else nnotfollow++;
                }
            }
            sw.Stop(); Console.WriteLine($"Build statistics ok. Duration={sw.ElapsedMilliseconds}");
            Console.WriteLine($"{nisolated} {nleftonly} {nrightonly} {nfull}");
            Console.WriteLine($"follow: {nfollow} not follow: {nnotfollow}");

            sw.Restart();
            ((StorageClientMaster)storage).ExtractChains();
            sw.Stop(); Console.WriteLine($"Extract chains ok. Duration={sw.ElapsedMilliseconds}");
        }
    }
}
