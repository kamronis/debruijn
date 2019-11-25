using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeBreinOrtho;

namespace DeBruijnNametable
{
    struct DBNode2
    {

    }
    partial class Program
    {
        public static void Main2(string[] args)
        {
            Console.WriteLine("Start DeBreinNametable Main2");

            //string readsfilename = @"D:\Home\data\deBrein\reads.txt";
            string tmpfilename = @"D:\Home\data\deBrein\tmp.bin";
            int portion = 20;


            // Построение потока ввода
            sw.Restart();
            TextReader treader = new StreamReader(File.Open(Options.readsfilename, FileMode.Open, FileAccess.Read));
            Stream tmpbinstream = File.Open(tmpfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(tmpbinstream);

            sw.Restart();
            string line = null;
            while ((line = treader.ReadLine()) != null)
            {
                for (int nom = 0; nom < line.Length - portion + 1; nom++)
                {
                    string word = line.Substring(nom, portion);
                    var cword = DBNode.Combine(word);
                    bw.Write(cword);
                }
            }
            bw.Flush();

            sw.Stop();
            Console.WriteLine($"Create binary tmp file ok. duration: {sw.ElapsedMilliseconds}");

            int nwords = (int)(tmpbinstream.Position / sizeof(UInt64));
            BinaryReader br = new BinaryReader(tmpbinstream);
            FileStream fs0 = File.Open(@"D:\Home\data\deBrein\tmp0.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream fs1 = File.Open(@"D:\Home\data\deBrein\tmp1.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream[] files = new FileStream[] { fs0, fs1 };
            FileStream filein, fileout;
            UInt64 nsec = 8;
            int code = 0;

            sw.Restart();

            for (UInt64 lay = 0; lay < nsec; lay++)
            {
                filein = files[lay & 1]; fileout = files[(lay + 1) & 1];
                BinaryReader binr = new BinaryReader(filein);
                BinaryWriter binw = new BinaryWriter(fileout);
                Dictionary<UInt64, int> dic = new Dictionary<ulong, int>();
                tmpbinstream.Position = 0L;
                filein.Position = 0L; fileout.Position = 0L;
                for (int i = 0; i < nwords; i++)
                {
                    UInt64 cw = br.ReadUInt64();
                    int nextcode = -3;
                    if (lay != 0) nextcode = binr.ReadInt32();
                    if ((cw & (nsec-1)) == lay)
                    {
                        if (dic.TryAdd(cw, code))
                        {
                            code++;
                        }
                        nextcode = code;
                    }
                    binw.Write(nextcode);
                }
                Console.WriteLine("memory: "+ GC.GetTotalMemory(false));
                dic = null;
                GC.Collect();
            }
            sw.Stop();
            Console.WriteLine($"Read binary tmp file ok. duration: {sw.ElapsedMilliseconds}");

        }
    }
}
