using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeBruijnNametable
{
    partial class Program
    {
        public static void Main42(string[] args)
        {
            Console.WriteLine("Start DeBruijnNametable Main42");

            sw.Restart();

            // Файл и поток бинарных ридов
            Stream breadstream = File.Open(Options.breadsfilename, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(breadstream);

            // Два альтернирующих файла, по очереди исполняющих роль входного и выходного потоков для кодированных ридов 
            FileStream fs0 = File.Open(@"D:\Home\data\deBrein\tmp0.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream fs1 = File.Open(@"D:\Home\data\deBrein\tmp1.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream[] files = new FileStream[] { fs0, fs1 };
            FileStream filein, fileout;
            
            graph.InitParts();

            // Создание cread
            sw.Restart();
            // Маска разрядов бинарного слова
            UInt64 mask = (UInt64)(Options.nparts - 1);

            // Кодирование узлов будем производить в несколько проходов (слоев). Выделим несколько битов в bword и на каждом 
            // проходе будем сравнивать с номером прохода (слоя)
            int nlays = 8;
            int lay;
            for (lay = 0; lay < nlays; lay++)
            {
                // Перемотаем на начало бинарный рид 
                breadstream.Position = 0L;
                // инициализируем входной и выходной стримы
                filein = files[lay & 1]; fileout = files[(lay + 1) & 1];
                BinaryReader binr = new BinaryReader(filein);
                BinaryWriter binw = new BinaryWriter(fileout);
                filein.Position = 0L; fileout.Position = 0L;

                // Теперь читаем, читаем, пишем, при первом слое входной стрим не читаем
                long nreeds = br.ReadInt64();
                if (lay > 0) binr.ReadInt64();
                binw.Write(nreeds);

                for (long ind = 0; ind < nreeds; ind++)
                {
                    // читаем и пишем длину бинарного рида
                    int nwords = (int)br.ReadInt64();
                    if (lay > 0) binr.ReadInt64();
                    binw.Write((long)nwords);

                    for (int nom = 0; nom < nwords; nom++)
                    {
                        // Читаем, читаем, пишем
                        UInt64 bword = br.ReadUInt64();
                        int code = -4;
                        if (lay > 0) code = binr.ReadInt32();
                        if (((bword >> Options.nshift) & (ulong)(nlays-1)) == (ulong)lay) 
                        {
                            code = graph.GetSetNode(bword);
                        }
                        binw.Write(code);
                    }
                }
                // Пошлем команду на освобождение словаря
                graph.DropDictionary();
                GC.Collect();
            }
            fs0.Close(); fs1.Close();
            string lastfname = (lay & 1) == 0 ? @"D:\Home\data\deBrein\tmp0.bin" : @"D:\Home\data\deBrein\tmp1.bin";
            if (File.Exists(Options.creadsfilename)) File.Delete(Options.creadsfilename);
            File.Move(lastfname, Options.creadsfilename);

            Console.WriteLine($"Memory used: {GC.GetTotalMemory(false)}");
            graph.MakePrototype();

            sw.Stop();

            //Console.WriteLine($"nodes.Count: {list.Count} Memory used {GC.GetTotalMemory(false)}");
            Console.WriteLine($"Create coded binary reeds file ok. duration: {sw.ElapsedMilliseconds}");
            breadstream.Close();
            //creadstream.Close();

        }
    }
}
