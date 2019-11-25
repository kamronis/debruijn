using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main4(string[] args)
        {
            Console.WriteLine("Start DeBruijnNametable Main4");

            //string breadsfilename = @"D:\Home\data\deBrein\breads.bin";
            //string creadsfilename = @"D:\Home\data\deBrein\creads.bin";
            //string nodelistfilename = @"D:\Home\data\deBrein\nlist.bin";

            sw.Restart();
            Stream breadstream = File.Open(Options.breadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(breadstream);
            Stream creadstream = File.Open(Options.creadsfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(creadstream);
            Stream nodeliststream = File.Open(Options.nodelistfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bwnl = new BinaryWriter(nodeliststream);


            Dictionary<UInt64, int> dic = new Dictionary<ulong, int>();
            List<CNode> list = new List<CNode>();

            // Создание прототипа cread
            sw.Restart();
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
                    int code = -3;// = list.Count;
                    if (!dic.TryGetValue(bword, out code))
                    {
                        code = list.Count;
                        dic.Add(bword, code);
                        list.Add(new CNode() { bword = bword }); 
                    }
                    bw.Write(code);
                }
            }
            bw.Flush();

            bwnl.Write((long)list.Count);
            foreach (var node in list)
            {
                bwnl.Write(node.bword);
                bwnl.Write(-1); // поля, которые в дальнейшем будут заполняться. -1 - null
                bwnl.Write(-1);
            }
            bwnl.Flush();
            sw.Stop();

            Console.WriteLine($"nodes.Count: {list.Count} Memory used {GC.GetTotalMemory(false)}");
            Console.WriteLine($"Create coded binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
            breadstream.Close();
            creadstream.Close();
            nodeliststream.Close();
        }
    }
}
