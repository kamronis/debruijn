using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DeBreinOrtho;

namespace DeBruijnNametable
{
    partial class Program
    {
        static void Main1(string[] args)
        {
            Console.WriteLine("Start DeBreinNametable");

            var dic = new Dictionary<UInt64, int>();
            var list = new List<DBNode>();

            int portion = 20;

            sw.Restart();
            using (TextReader reader = new StreamReader(File.Open(Options.readsfilename, FileMode.Open, FileAccess.Read)))
            {
                int lcount = 0;
                int nwords = 0;
                int code = 0;
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    lcount++;
                    for (int nom = 0; nom < line.Length - portion + 1; nom++)
                    {
                        string word = line.Substring(nom, portion);
                        var cword = DBNode.Combine(word);
                        nwords++;

                        if (dic.TryAdd(cword, code))
                        {
                            list.Add(new DBNode(code, cword));
                            code++;
                        }
                    }
                }
                System.GC.Collect();
                Console.WriteLine($"GC.GetTotalMemory(true): {System.GC.GetTotalMemory(true)}");
                //nt.Flush();
                Console.WriteLine($"lines:{lcount} words: {nwords} nodes: {list.Count}");
                //nt.Build();
            }
            sw.Stop();
            Console.WriteLine($"ok. duration: {sw.ElapsedMilliseconds}");

        }
    }
}
