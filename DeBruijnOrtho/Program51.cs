using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijn
{
    partial class Program
    {
        public static void Main51()
        {
            Console.WriteLine("Start Main51");

            sw.Restart();
            Stream creadstream = File.Open(Options.creadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(creadstream);

            //NodesPart[] parts = new NodesPart[] { new NodesPart(@"D:\Home\data\deBrein\nlist.bin") };
            graph.InitParts();
            Console.WriteLine("graph.InitParts() ok.");
            graph.Restore51();
            Console.WriteLine("graph.Restore51() ok.");

            // Обработаем риды, сформируем граф
            sw.Restart();
            long nreeds = br.ReadInt64();
            for (long ind = 0; ind < nreeds; ind++)
            {
                if (ind % 1000_000 == 0) Console.Write($"{ind / 1000_000} ");
                // читаем длину бинарного рида
                int nwords = (int)br.ReadInt64();
                int codeprev = -1;
                for (int nom = 0; nom < nwords; nom++)
                {
                    int code = br.ReadInt32();
                    if (nom != 0) // точно есть текущий и предыдущий узлы
                    {
                        graph.SetNodePrev(code, codeprev);
                        graph.SetNodeNext(codeprev, code);
                    }
                    codeprev = code;
                }
            }

            graph.Save();
            graph.Close();

            sw.Stop();
            Console.WriteLine($"Create Graph ok. duration: {sw.ElapsedMilliseconds}");
        }
    }

}
