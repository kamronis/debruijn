using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main41(string[] args)
        {
            Console.WriteLine("Start DeBruijnNametable Main41");

            sw.Restart();
            Stream breadstream = File.Open(Options.breadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(breadstream);
            Stream creadstream = File.Open(Options.creadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(creadstream);


            //NodesPart[] parts = new NodesPart[] { new NodesPart(options.nodelistfilename) };
            graph.InitParts();

            // Создание cread
            sw.Restart();
            UInt64 mask = (UInt64)(Options.nparts - 1);
            long nreeds = br.ReadInt64();
            bw.Write(nreeds);
            for (long ind = 0; ind < nreeds; ind++)
            {
                // читаем и пишем длину бинарного рида
                int nwords = (int)br.ReadInt64();
                bw.Write((long)nwords);
                for (int nom = 0; nom < nwords; nom++)
                {
                    UInt64 bword = br.ReadUInt64();
                    int code = graph.GetSetNode(bword);
                    bw.Write(code);
                }
            }
            bw.Flush();

            Console.WriteLine($"Memory used: {GC.GetTotalMemory(false)}");
            graph.MakePrototype();
            graph.Close();
            
            sw.Stop();

            //Console.WriteLine($"nodes.Count: {list.Count} Memory used {GC.GetTotalMemory(false)}");
            Console.WriteLine($"Create coded binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
            breadstream.Close();
            creadstream.Close();

        }
    }
}
